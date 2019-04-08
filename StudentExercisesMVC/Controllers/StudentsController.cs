using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercisesMVC.Models;

namespace StudentExercisesMVC.Controllers {

    public class StudentsController : Controller {

        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config) {

            _config = config;
        }

        public SqlConnection Connection {

            get {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // GET: Students
        public ActionResult Index(string q, string firstName = "", string lastName = "", string slackHandle = "", string include = "") {

            string searchFN = (firstName == "") ? "%" : firstName;
            string searchLN = (lastName == "") ? "%" : lastName;
            string searchSH = (slackHandle == "") ? "%" : slackHandle;

            if (include != "exercises") {

                List<Student> students = new List<Student>();

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"SELECT s.id AS studentId, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, 
                                                    c.CohortName, c.id AS chrtId 
                                               FROM Student s 
                                               JOIN Cohort c ON s.CohortId = c.id
                                              WHERE s.FirstName LIKE '{searchFN}' AND s.LastName LIKE '{searchLN}' 
                                                     AND s.SlackHandle LIKE '{searchSH}'";

                        if (!string.IsNullOrWhiteSpace(q)) {
                            cmd.CommandText += @" AND (s.FirstName LIKE @q OR s.LastName LIKE @q OR s.SlackHandle LIKE @q)";
                            cmd.Parameters.Add(new SqlParameter("@q", $"%{q}%"));
                        }

                        SqlDataReader reader = cmd.ExecuteReader();


                        while (reader.Read()) {

                            Student student = new Student(reader.GetInt32(reader.GetOrdinal("studentId")),
                                reader.GetString(reader.GetOrdinal("FirstName")),
                                reader.GetString(reader.GetOrdinal("LastName")),
                                reader.GetString(reader.GetOrdinal("SlackHandle")),
                                reader.GetInt32(reader.GetOrdinal("CohortId")))
                            {
                                Cohort = new Cohort(
                                    reader.GetInt32(reader.GetOrdinal("chrtId")),
                                    reader.GetString(reader.GetOrdinal("CohortName")))
                            };
                            students.Add(student);
                        }
                        reader.Close();
                        return View(students);
                    }
                }

            } else {

                using (SqlConnection conn = Connection) {

                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"SELECT s.id AS StudentId, s.FirstName, s.LastName, 
                                                    s.SlackHandle, s.CohortId, 
                                                    c.CohortName, c.id AS ChrtId, e.id AS ExerciseId, 
                                                    e.ExerciseName, e.ExerciseLanguage
                                               FROM Student s
                                               JOIN Cohort c ON s.CohortId = c.id
                                               JOIN AssignedExercise ae ON ae.StudentID = s.Id
                                               JOIN Exercise e ON ae.ExerciseId = e.id
                                              WHERE (s.FirstName LIKE '{searchFN}' 
                                                    AND s.LastName LIKE '{searchLN}' 
                                                    AND s.SlackHandle LIKE '{searchSH}')
                                           ORDER BY StudentId, ExerciseId";

                        SqlDataReader reader = cmd.ExecuteReader();

                        List<Student> students = new List<Student>();

                        while (reader.Read()) {

                            int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));

                            Student student = students.FirstOrDefault(s => s.Id == studentId);

                            if (student == null) {

                                student = new Student(reader.GetInt32(reader.GetOrdinal("StudentId")),
                                reader.GetString(reader.GetOrdinal("FirstName")),
                                reader.GetString(reader.GetOrdinal("LastName")),
                                reader.GetString(reader.GetOrdinal("SlackHandle")),
                                reader.GetInt32(reader.GetOrdinal("CohortId"))) {
                                    Cohort = new Cohort(
                                    reader.GetInt32(reader.GetOrdinal("ChrtId")),
                                    reader.GetString(reader.GetOrdinal("CohortName"))),
                                    AssignedExercises = new List<Exercise>()
                                };
                                students.Add(student);
                            } 

                            if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId"))) {

                                int exerciseId = reader.GetInt32(reader.GetOrdinal("ExerciseId"));

                                if (!student.AssignedExercises.Any(e => e.Id == exerciseId)) {

                                    Exercise exercise = new Exercise(reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                        reader.GetString(reader.GetOrdinal("ExerciseName")),
                                        reader.GetString(reader.GetOrdinal("ExerciseLanguage")));

                                    student.AssignedExercises.Add(exercise);
                                }
                            }
                        }
                        reader.Close();
                        return View(students);
                    }
                }
            }
        }

        // GET: Students/Details/5
        public ActionResult Details(int id, string include = "") {

            if (include != "exercises") {

                Student student = null;

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"SELECT s.id AS studentId, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, 
                                                    c.CohortName, c.id AS chrtId
                                               FROM Student s 
                                               JOIN Cohort c ON s.CohortId = c.id
                                              WHERE s.Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        SqlDataReader reader = cmd.ExecuteReader();


                        if (reader.Read()) {

                            student = new Student(reader.GetInt32(reader.GetOrdinal("studentId")),
                               reader.GetString(reader.GetOrdinal("FirstName")),
                               reader.GetString(reader.GetOrdinal("LastName")),
                               reader.GetString(reader.GetOrdinal("SlackHandle")),
                               reader.GetInt32(reader.GetOrdinal("CohortId"))) {
                                Cohort = new Cohort(
                                   reader.GetInt32(reader.GetOrdinal("chrtId")),
                                   reader.GetString(reader.GetOrdinal("CohortName")))
                                };
                        }
                        reader.Close();
                        return View(student);
                    }
                }

            } else {

                Student student = null;

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"SELECT s.id AS StudentId, s.FirstName, s.LastName, 
                                                    s.SlackHandle, s.CohortId, 
                                                    c.CohortName, c.id AS chrtId, e.id AS ExerciseId, 
                                                    e.ExerciseName, e.ExerciseLanguage
                                               FROM Student s
                                               JOIN Cohort c ON s.CohortId = c.id
                                               JOIN AssignedExercise ae ON ae.StudentID = s.id
                                               JOIN Exercise e ON ae.ExerciseId = e.id
                                              WHERE StudentId = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        SqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read()) {

                            if (student == null) {

                                student = new Student(reader.GetInt32(reader.GetOrdinal("StudentId")),
                                   reader.GetString(reader.GetOrdinal("FirstName")),
                                   reader.GetString(reader.GetOrdinal("LastName")),
                                   reader.GetString(reader.GetOrdinal("SlackHandle")),
                                   reader.GetInt32(reader.GetOrdinal("CohortId"))) {
                                    Cohort = new Cohort(
                                       reader.GetInt32(reader.GetOrdinal("chrtId")),
                                       reader.GetString(reader.GetOrdinal("CohortName"))),
                                    AssignedExercises = new List<Exercise>()
                                };

                                if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId"))) {

                                    int exerciseId = reader.GetInt32(reader.GetOrdinal("ExerciseId"));

                                    if (!student.AssignedExercises.Any(e => e.Id == exerciseId)) {

                                        Exercise exercise = new Exercise(reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                            reader.GetString(reader.GetOrdinal("ExerciseName")),
                                            reader.GetString(reader.GetOrdinal("ExerciseLanguage")));

                                        student.AssignedExercises.Add(exercise);
                                    }
                                }
                            }
                        }
                        reader.Close();
                    }
                    return View(student);
                }
            }
        }
        
        // GET: Students/Create
        public ActionResult Create(Student student) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"INSERT INTO Student (FirstName, LastName, SlackHandle, CohortId) 
                                         OUTPUT INSERTED.Id
                                         VALUES (@firstName, @lastName, @slackHandle, @cohortId);
                                         SELECT MAX(Id) 
                                           FROM Student";

                    cmd.Parameters.Add(new SqlParameter("@firstName", student.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", student.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    student.Id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, student);
                }
            }
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection) {
            try {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch {
                return View();
            }
        }

        // GET: Students/Edit/5
        public ActionResult Edit(int id) {
            return View();
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection) {
            try {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch {
                return View();
            }
        }

        // GET: Students/Delete/5
        public ActionResult Delete(int id) {
            return View();
        }

        // POST: Students/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection) {
            try {
                
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch {
                return View();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercisesMVC.Models;
using StudentExercisesMVC.Models.ViewModels;

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
        public ActionResult Create() {

            StudentCreateViewModel viewModel = 
                new StudentCreateViewModel(_config.GetConnectionString("DefaultConnection"));
            return View(viewModel);
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentCreateViewModel viewModel) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"INSERT INTO Student (FirstName, LastName, SlackHandle, CohortId) 
                                         OUTPUT INSERTED.Id
                                         VALUES (@firstName, @lastName, @slackHandle, @cohortId);
                                         SELECT MAX(Id) 
                                           FROM Student";

                        cmd.Parameters.Add(new SqlParameter("@firstName", viewModel.Student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", viewModel.Student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", viewModel.Student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", viewModel.Student.CohortId));

                        cmd.ExecuteNonQuery();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch {

                return View();
            }
        }

        // GET: Students/Edit/5
        public ActionResult Edit(int id) {

            Student student = GetStudentById(id);

            if (student == null) {
                return NotFound();
            }

            StudentEditViewModel viewModel = new StudentEditViewModel {
                Cohorts = GetAllCohorts(),
                Student = student
            };

            return View(viewModel);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, StudentEditViewModel viewModel) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = @"UPDATE student 
                                           SET firstname = @firstname, 
                                               lastname = @lastname,
                                               slackhandle = @slackhandle, 
                                               cohortid = @cohortid
                                         WHERE id = @id;";

                        cmd.Parameters.Add(new SqlParameter("@firstname", viewModel.Student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastname", viewModel.Student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackhandle", viewModel.Student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortid", viewModel.Student.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch {

                viewModel.Cohorts = GetAllCohorts();
                return View(viewModel);
            }
        }

        // GET: Students/Delete/5
        public ActionResult Delete(int id) {

            Student student = GetStudentById(id);

            if (student == null) {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Student student) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"DELETE FROM Student 
                                              WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0) {

                            return RedirectToAction(nameof(Index));
                        }

                        throw new Exception("No rows affected");
                    }
                }
            } catch (Exception) {

                if (!StudentExists(id)) {

                    return NotFound();
                }

                throw;
            }
        }

        // GET: /Students/AssignExercises/1
        public ActionResult AssignExercises(int id) {

            Student student = GetStudentById(id);

            if (student == null) {
                return NotFound();
            }

            StudentAssignExercisesViewModel viewModel = new StudentAssignExercisesViewModel {
                Student = student,
                ExercisesNotAssignedToStudent = GetUnassignedExercises(student.Id),
                AssignedExercises = GetAssignedExercises(student.Id),
                UnassignedExercises = null
            };

            return View(viewModel);
        }

        // POST: Students/AssignExercises/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignExercises(int id, StudentAssignExercisesViewModel viewModel) {

            try {

                foreach (var item in viewModel.UnassignedExercises) {

                    int exerciseId = int.Parse(item);

                    using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                            cmd.CommandText = $@"INSERT INTO AssignedExercise(StudentId, ExerciseId)
                                                      OUTPUT INSERTED.Id
                                                      VALUES(@studentId, @exerciseId);
                                                      SELECT MAX(Id)
                                                        FROM AssignedExercise;";

                            cmd.Parameters.Add(new SqlParameter("@studentId", id));
                            cmd.Parameters.Add(new SqlParameter("@exerciseId", exerciseId));

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                 return RedirectToAction(nameof(Index));

            } catch {

                return View(viewModel);
            }
        }

        private bool StudentExists(int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT Id, FirstName, LastName, SlackHandle, CohortId 
                                           FROM Student 
                                          WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

        private Student GetStudentById(int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = @"SELECT s.Id AS StudentId,
                                               s.FirstName, s.LastName, 
                                               s.SlackHandle, s.CohortId,
                                               c.CohortName AS CohortName
                                          FROM Student s LEFT JOIN Cohort c on s.cohortid = c.id
                                         WHERE  s.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;

                    if (reader.Read()) {
                        student = new Student {
                            Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                CohortName = reader.GetString(reader.GetOrdinal("CohortName")),
                            }
                        };
                    }
                    reader.Close();
                    return student;
                }
            }
        }

        private List<Cohort> GetAllCohorts() {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = @"SELECT id, cohortname from Cohort;";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Cohort> cohorts = new List<Cohort>();

                    while (reader.Read()) {
                        cohorts.Add(new Cohort {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            CohortName = reader.GetString(reader.GetOrdinal("cohortname"))
                        });
                    }
                    reader.Close();
                    return cohorts;
                }
            }
        }

        private List<Exercise> GetUnassignedExercises(int studentId) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = @"SELECT e.Id AS ExerciseId, e.ExerciseName, e.ExerciseLanguage
                                          FROM Exercise e
                                     LEFT JOIN (SELECT e.Id AS ExerciseId, e.ExerciseName, 
                                                       e.ExerciseLanguage, s.Id AS StudentID
                                                  FROM AssignedExercise ae
                                                  JOIN Exercise e ON e.Id = ae.ExerciseId
                                                  JOIN Student s ON s.Id = ae.StudentId
                                                 WHERE s.Id = @studentId) ae ON ae.ExerciseId = e.Id
                                        WHERE ae.ExerciseId IS NULL;";

                    cmd.Parameters.Add(new SqlParameter("@studentId", studentId));
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Exercise> exercises = new List<Exercise>();

                    while (reader.Read()) {
                        exercises.Add(new Exercise {
                            Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                            ExerciseName = reader.GetString(reader.GetOrdinal("ExerciseName")),
                            ExerciseLanguage = reader.GetString(reader.GetOrdinal("ExerciseLanguage"))
                        });
                    }
                    reader.Close();
                    return exercises;
                }
            }
        }

        private List<Exercise> GetAssignedExercises(int studentId) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = @"SELECT e.Id AS ExerciseId, e.ExerciseName, e.ExerciseLanguage
                                          FROM AssignedExercise ae
                                          JOIN Exercise e ON e.Id = ae.ExerciseId
                                          JOIN Student s ON s.Id = ae.StudentId
                                         WHERE s.Id = @studentId;";

                    cmd.Parameters.Add(new SqlParameter("@studentId", studentId));
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Exercise> exercises = new List<Exercise>();

                    while (reader.Read()) {
                        exercises.Add(new Exercise {
                            Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                            ExerciseName = reader.GetString(reader.GetOrdinal("ExerciseName")),
                            ExerciseLanguage = reader.GetString(reader.GetOrdinal("ExerciseLanguage"))
                        });
                    }
                    reader.Close();
                    return exercises;
                }
            }
        }
    }
}
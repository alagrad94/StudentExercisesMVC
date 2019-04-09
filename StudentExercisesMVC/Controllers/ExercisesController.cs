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

    public class ExercisesController : Controller {

        private readonly IConfiguration _config;

        public ExercisesController(IConfiguration config) {

            _config = config;
        }

        public SqlConnection Connection {

            get {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }

        }
        // GET: Exercises
        public ActionResult Index(string q, string name = "", string language = "", string include = "") {
            string searchName = (name == "") ? "%" : name;
            string searchLang = (language == "") ? "%" : language;

            if (include != "students") {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"SELECT id, ExerciseName, ExerciseLanguage 
                                               FROM Exercise 
                                              WHERE ExerciseLanguage LIKE '{searchLang}' AND ExerciseName LIKE '{searchName}'";

                        if (!string.IsNullOrWhiteSpace(q)) {
                            cmd.CommandText += @" AND (ExerciseName LIKE @q OR ExerciseLanguage LIKE @q)";
                            cmd.Parameters.Add(new SqlParameter("@q", $"%{q}%"));
                        }

                        SqlDataReader reader = cmd.ExecuteReader();

                        List<Exercise> exercises = new List<Exercise>();

                        while (reader.Read()) {

                            Exercise exercise = new Exercise(reader.GetInt32(reader.GetOrdinal("id")),
                                reader.GetString(reader.GetOrdinal("ExerciseName")),
                                reader.GetString(reader.GetOrdinal("ExerciseLanguage")));

                            exercises.Add(exercise);
                        }
                        reader.Close();
                        return View(exercises);
                    }
                }
            } else {

                List<Exercise> exercises = new List<Exercise>();

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"SELECT id, ExerciseName, ExerciseLanguage 
                                               FROM Exercise 
                                              WHERE (ExerciseLanguage LIKE '{searchLang}' AND ExerciseName LIKE '{searchName}')";

                        SqlDataReader reader = cmd.ExecuteReader();


                        while (reader.Read()) {

                            Exercise exercise = new Exercise(reader.GetInt32(reader.GetOrdinal("id")),
                                reader.GetString(reader.GetOrdinal("ExerciseName")),
                                reader.GetString(reader.GetOrdinal("ExerciseLanguage")));

                            exercises.Add(exercise);
                        }

                        reader.Close();

                        foreach (Exercise exercise in exercises) {

                            cmd.CommandText = $@"SELECT s.id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId
                                                   FROM AssignedExercise ae 
                                                   JOIN Student s ON ae.StudentId = s.id
                                                  WHERE ae.ExerciseId = {exercise.Id}";

                            SqlDataReader reader2 = cmd.ExecuteReader();

                            while (reader2.Read()) {

                                Student student = new Student(reader2.GetInt32(reader2.GetOrdinal("id")),
                                 reader2.GetString(reader2.GetOrdinal("FirstName")),
                                 reader2.GetString(reader2.GetOrdinal("LastName")),
                                 reader2.GetString(reader2.GetOrdinal("SlackHandle")),
                                 reader2.GetInt32(reader2.GetOrdinal("CohortId")));

                                exercise.ExerciseStudents.Add(student);
                            }
                            reader2.Close();
                        }
                        return View(exercises);
                    }
                }
            }
        }

        // GET: Exercises/Details/5
        public ActionResult Details(int id, string include = "") {

            if (include != "students") {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"SELECT id, ExerciseName, ExerciseLanguage 
                                           FROM Exercise 
                                          WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();

                        Exercise exercise = null;

                        while (reader.Read()) {

                            exercise = new Exercise(reader.GetInt32(reader.GetOrdinal("id")),
                                reader.GetString(reader.GetOrdinal("ExerciseName")),
                                reader.GetString(reader.GetOrdinal("ExerciseLanguage")));
                        }

                        reader.Close();
                        return View(exercise);
                    }
                }
            } else {

                Exercise exercise = null;

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"SELECT id, ExerciseName, ExerciseLanguage 
                                               FROM Exercise 
                                              WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        SqlDataReader reader = cmd.ExecuteReader();


                        while (reader.Read()) {

                            exercise = new Exercise(reader.GetInt32(reader.GetOrdinal("id")),
                                reader.GetString(reader.GetOrdinal("ExerciseName")),
                                reader.GetString(reader.GetOrdinal("ExerciseLanguage")));
                        }

                        reader.Close();

                        cmd.CommandText = $@"SELECT s.id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId
                                               FROM AssignedExercise ae 
                                               JOIN Student s ON ae.StudentId = s.id
                                              WHERE ae.ExerciseId = {exercise.Id}";

                        SqlDataReader reader2 = cmd.ExecuteReader();

                        while (reader2.Read()) {

                            Student student = new Student(reader2.GetInt32(reader2.GetOrdinal("id")),
                             reader2.GetString(reader2.GetOrdinal("FirstName")),
                             reader2.GetString(reader2.GetOrdinal("LastName")),
                             reader2.GetString(reader2.GetOrdinal("SlackHandle")),
                             reader2.GetInt32(reader2.GetOrdinal("CohortId")));

                            exercise.ExerciseStudents.Add(student);
                        }

                        reader2.Close();
                        return View(exercise);
                    }
                }
            }
        }

        // GET: Exercises/Create
        public ActionResult Create() {

            Exercise exercise = new Exercise();
            return View(exercise);
        }

        // POST: Exercises/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Exercise exercise) {

            try {
                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"INSERT INTO Exercise (ExerciseName, ExerciseLanguage)
                                         OUTPUT INSERTED.Id
                                         VALUES (@exerciseName, @exerciseLanguage)
                                         SELECT MAX(Id) 
                                           FROM Exercise";

                        cmd.Parameters.Add(new SqlParameter("@exerciseName", exercise.ExerciseName));
                        cmd.Parameters.Add(new SqlParameter("@exerciseLanguage", exercise.ExerciseLanguage));

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }
            } catch {

                return View();
            }
        }

        // GET: Exercises/Edit/5
        public ActionResult Edit(int id) {

            Exercise exercise = GetExerciseById(id);

            if (exercise == null) {
                return NotFound();
            }

            return View(exercise);
        }

        // POST: Exercises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Exercise exercise) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"UPDATE Exercise
                                                SET ExerciseName = @exerciseName, ExerciseLanguage = @exerciseLanguage
                                              WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@exerciseName", exercise.ExerciseName));
                        cmd.Parameters.Add(new SqlParameter("@exerciseLanguage", exercise.ExerciseLanguage));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0) {

                            return RedirectToAction(nameof(Index));
                        }

                        throw new Exception("No rows affected");
                    }
                }
            } catch (Exception) {

                if (!ExerciseExists(id)) {

                    return NotFound();
                }

                throw;
            }
        }

        // GET: Exercises/Delete/5
        public ActionResult Delete(int id) {

            Exercise exercise = GetExerciseById(id);

            if (exercise == null) {
                return NotFound();
            }

            return View(exercise);
        }

        // POST: Exercises/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Exercise exercise) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"DELETE FROM Exercise 
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

                if (!ExerciseExists(id)) {

                    return NotFound();
                }

                throw;
            }
        }

        private Exercise GetExerciseById(int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = @"SELECT Id, ExerciseName, ExerciseLanguage
                                          FROM Exercise
                                         WHERE  Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Exercise exercise = null;

                    if (reader.Read()) {
                        exercise = new Exercise {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            ExerciseName = reader.GetString(reader.GetOrdinal("ExerciseName")),
                            ExerciseLanguage = reader.GetString(reader.GetOrdinal("ExerciseLanguage"))
                        };
                    }
                    reader.Close();
                    return exercise;
                }
            }
        }

        private bool ExerciseExists(int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT Id, ExerciseName, ExerciseLanguage 
                                           FROM Exercise 
                                          WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
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
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Exercises/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Exercises/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Exercises/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Exercises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Exercises/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Exercises/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
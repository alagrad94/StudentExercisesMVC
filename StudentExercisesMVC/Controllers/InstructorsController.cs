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

    public class InstructorsController : Controller {

        private readonly IConfiguration _config;

        public InstructorsController(IConfiguration config) {

            _config = config;
        }

        public SqlConnection Connection {

            get {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }

        }


        // GET: Instructors
        public ActionResult Index(string q, string firstName = "", string lastName = "", string slackHandle = "", string include = "") {

            string searchFN = (firstName == "") ? "%" : firstName;
            string searchLN = (lastName == "") ? "%" : lastName;
            string searchSH = (slackHandle == "") ? "%" : slackHandle;

            List<Instructor> instructors = new List<Instructor>();

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT i.id AS instId, i.FirstName, i.LastName, i.SlackHandle, i.CohortId, 
                                                c.CohortName, c.id AS chrtId
                                           FROM Instructor i 
                                           JOIN Cohort c ON i.CohortId = c.id
                                          WHERE i.FirstName LIKE '{searchFN}' AND i.LastName LIKE '{searchLN}' 
                                                AND i.SlackHandle LIKE '{searchSH}'";

                    if (!string.IsNullOrWhiteSpace(q)) {
                        cmd.CommandText += @" AND (i.FirstName LIKE @q OR i.LastName LIKE @q OR i.SlackHandle LIKE @q)";
                        cmd.Parameters.Add(new SqlParameter("@q", $"%{q}%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read()) {

                        Instructor instructor = new Instructor(reader.GetInt32(reader.GetOrdinal("instId")),
                            reader.GetString(reader.GetOrdinal("FirstName")),
                            reader.GetString(reader.GetOrdinal("LastName")),
                            reader.GetString(reader.GetOrdinal("SlackHandle")),
                            reader.GetInt32(reader.GetOrdinal("CohortId"))) {
                            Cohort = new Cohort(
                                reader.GetInt32(reader.GetOrdinal("chrtId")),
                                reader.GetString(reader.GetOrdinal("CohortName")))
                        };

                        instructors.Add(instructor);
                    }

                    reader.Close();
                    return View(instructors);
                }
            }
        }


        // GET: Instructors/Details/5
        public ActionResult Details(int id) {
            return View();
        }

        // GET: Instructors/Create
        public ActionResult Create() {
            return View();
        }

        // POST: Instructors/Create
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

        // GET: Instructors/Edit/5
        public ActionResult Edit(int id) {
            return View();
        }

        // POST: Instructors/Edit/5
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

        // GET: Instructors/Delete/5
        public ActionResult Delete(int id) {
            return View();
        }

        // POST: Instructors/Delete/5
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
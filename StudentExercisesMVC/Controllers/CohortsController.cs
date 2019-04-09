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

    public class CohortsController : Controller {

        private readonly IConfiguration _config;

        public CohortsController(IConfiguration config) {

            _config = config;
        }

        public SqlConnection Connection {

            get {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }

        }
        // GET: Cohorts
        public ActionResult Index(string q, string name = "") { 

            string searchName = (name == "") ? "%" : name;

            List<Cohort> cohorts = new List<Cohort>();

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT id, CohortName 
                                           FROM Cohort 
                                          WHERE CohortName LIKE '{searchName}'";

                    if (!string.IsNullOrWhiteSpace(q)) {
                        cmd.CommandText += @" AND CohortName LIKE @q";
                        cmd.Parameters.Add(new SqlParameter("@q", $"%{q}%"));
                    }

                SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read()) {

                        Cohort cohort = new Cohort(reader.GetInt32(reader.GetOrdinal("id")),
                            reader.GetString(reader.GetOrdinal("CohortName")));

                        cohorts.Add(cohort);
                    }

                    reader.Close();

                    foreach (Cohort cohort in cohorts) {

                        cmd.CommandText = $@"SELECT s.id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId 
                                               FROM Student s
                                          LEFT JOIN Cohort c on s.CohortId = c.id
                                              WHERE c.id = {cohort.Id}";

                        SqlDataReader reader2 = cmd.ExecuteReader();

                        while (reader2.Read()) {

                            Student student = new Student(reader2.GetInt32(reader2.GetOrdinal("id")),
                                 reader2.GetString(reader2.GetOrdinal("FirstName")),
                                 reader2.GetString(reader2.GetOrdinal("LastName")),
                                 reader2.GetString(reader2.GetOrdinal("SlackHandle")),
                                 reader2.GetInt32(reader2.GetOrdinal("CohortId")));

                            cohort.StudentList.Add(student);
                        }

                        reader2.Close();
                    }

                    foreach (Cohort cohort in cohorts) {

                        cmd.CommandText = $@"SELECT i.id, i.FirstName, i.LastName, i.SlackHandle, i.CohortId 
                                               FROM Instructor i
                                          LEFT JOIN Cohort c on i.CohortId = c.id
                                              WHERE c.id = {cohort.Id}";

                        SqlDataReader reader3 = cmd.ExecuteReader();

                        while (reader3.Read()) {

                            Instructor instructor = new Instructor(reader3.GetInt32(reader3.GetOrdinal("id")),
                             reader3.GetString(reader3.GetOrdinal("FirstName")),
                             reader3.GetString(reader3.GetOrdinal("LastName")),
                             reader3.GetString(reader3.GetOrdinal("SlackHandle")),
                             reader3.GetInt32(reader3.GetOrdinal("CohortId")));

                            cohort.InstructorList.Add(instructor);
                        }

                        reader3.Close();
                    }
                }
                return View(cohorts);
            }
        }

        // GET: Cohorts/Details/5
        public ActionResult Details(int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT c.id AS CohortId, c.CohortName AS CohortName, 
                                                s.id AS StudentId, s.FirstName AS StudentFirstName, s.LastName AS StudentLastName, 
                                                s.SlackHandle AS StudentSlackHandle,
                                                i.id AS InstructorId, i.FirstName AS InstructorFirstName, i.LastName AS InstructorLastName, 
                                                i.SlackHandle AS InstructorSlackHandle
                                           FROM Cohort c
                                     LEFT JOIN Student s ON c.id = s.CohortId
                                     LEFT JOIN Instructor i ON c.id = i.CohortId
                                          WHERE c.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;

                    while (reader.Read()) {

                        if (cohort == null) {

                            cohort = new Cohort(reader.GetInt32(reader.GetOrdinal("CohortId")),
                                reader.GetString(reader.GetOrdinal("CohortName")));
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("StudentId"))) {
                            int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));

                            if (!cohort.StudentList.Any(s => s.Id == studentId)) {

                                Student student = new Student(studentId,
                                    reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                    reader.GetInt32(reader.GetOrdinal("CohortId")));

                                cohort.StudentList.Add(student);

                            }
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("InstructorId"))) {

                            int instructorId = reader.GetInt32(reader.GetOrdinal("InstructorId"));

                            if (!cohort.InstructorList.Any(i => i.Id == instructorId)) {

                                Instructor instructor = new Instructor(instructorId,
                                    reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                                    reader.GetInt32(reader.GetOrdinal("CohortId")));

                                cohort.InstructorList.Add(instructor);
                            }

                        }
                    }

                    reader.Close();
                    return View(cohort);
                }
            }
        }

        // GET: Cohorts/Create
        public ActionResult Create() {

            Cohort cohort = new Cohort();
            return View(cohort);
        }

        // POST: Cohorts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Cohort cohort) {

            try {
                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"INSERT INTO Cohort (CohortName)
                                                  OUTPUT INSERTED.Id
                                                  VALUES (@cohortName) 
                                                  SELECT MAX(Id) 
                                                    FROM Cohort";

                        cmd.Parameters.Add(new SqlParameter("@cohortName", cohort.CohortName));

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }
            } catch {

                return View();
            }
        }

        // GET: Cohorts/Edit/5
        public ActionResult Edit(int id) {

            Cohort cohort = GetCohortById(id);

            if (cohort == null) {
                return NotFound();
            }

            return View(cohort);
        }

        // POST: Cohorts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Cohort cohort) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = @"UPDATE Cohort
                                               SET CohortName = @cohortName
                                             WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@cohortName", cohort.CohortName));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0) {

                            return RedirectToAction(nameof(Index));
                        }

                        throw new Exception("No rows affected");
                    }
                }
            } catch (Exception) {

                if (!CohortExists(id)) {

                    return NotFound();
                }

                throw;
            }
        }

        // GET: Cohorts/Delete/5
        public ActionResult Delete(int id) {

            Cohort cohort = GetCohortById(id);

            if (cohort == null) {
                return NotFound();
            }

            return View(cohort);
        }

        // POST: Cohorts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Cohort cohort) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"DELETE FROM Cohort 
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

                if (!CohortExists(id)) {

                    return NotFound();
                }

                throw;
            }
        }

        private Cohort GetCohortById(int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = @"SELECT Id, CohortName
                                          FROM Cohort
                                         WHERE  Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;

                    if (reader.Read()) {
                        cohort = new Cohort {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            CohortName = reader.GetString(reader.GetOrdinal("CohortName"))
                        };
                    }
                    reader.Close();
                    return cohort;
                }
            }
        }

        private bool CohortExists(int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT Id, CohortName 
                                           FROM Cohort 
                                          WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models.ViewModels {

    public class InstructorCreateViewModel {

        public InstructorCreateViewModel() {
            Cohorts = new List<Cohort>();
        }

        public InstructorCreateViewModel(string connectionString) {
            using (SqlConnection conn = new SqlConnection(connectionString)) {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = @"SELECT id, cohortname from Cohort;";
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohorts = new List<Cohort>();

                    while (reader.Read()) {
                        Cohorts.Add(new Cohort {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            CohortName = reader.GetString(reader.GetOrdinal("cohortname"))
                        });
                    }
                    reader.Close();
                }
            }
        }


        public Instructor Instructor { get; set; }
        public List<Cohort> Cohorts { get; set; }

        public List<SelectListItem> CohortOptions {
            get {
                return Cohorts.Select(c => new SelectListItem {
                    Value = c.Id.ToString(),
                    Text = c.CohortName
                }).ToList();
            }
        }
    }
}

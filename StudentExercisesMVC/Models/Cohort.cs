using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentExercisesMVC.Models { 

    public class Cohort {

        public Cohort() {

            CohortName = null;
        }

        public Cohort (int id, string cohortName) {

            Id = id;
            CohortName = cohortName;
            StudentList = new List<Student>();
            InstructorList = new List<Instructor>();
        }

       public Cohort(string cohortName) {

            CohortName = cohortName;
            StudentList = new List<Student>();
            InstructorList = new List<Instructor>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(11, MinimumLength = 5)]
        [RegularExpression(@"(\bday\b|\bDay\b|\bevening\b|\bEvening\b)\s(\b\d{1,2})")]
        public string CohortName { get; set; }

        public List<Student> StudentList { get; set; }

        public List<Instructor> InstructorList { get; set; }
    }
}

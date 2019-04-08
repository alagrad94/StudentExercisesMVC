using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentExercisesMVC.Models {

    public class Instructor {

        public Instructor (int id, string firstName, string lastName, string slackHandle, int cohortId) {
      
          Id = id;
          FirstName = firstName;
          LastName = lastName;
          SlackHandle = slackHandle;
          CohortId = cohortId;
          Cohort = new Cohort();
        }

        public Instructor(string firstName, string lastName, string slackHandle, int cohortId) {
      
            FirstName = firstName;
            LastName = lastName;
            SlackHandle = slackHandle;
            CohortId = cohortId;
            Cohort = new Cohort();
        }

        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        [StringLength(25, MinimumLength = 2)]
        public string LastName { get; set; }

        [Required]
        [StringLength(12, MinimumLength = 3)]
        public string SlackHandle { get; set; }

        [Required]
        public int CohortId { get; set; }

        public Cohort Cohort { get; set; }

    }

}

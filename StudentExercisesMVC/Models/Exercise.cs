using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentExercisesMVC.Models { 

  public class Exercise {

        public Exercise() {

            Id = 0;
            ExerciseName = null;
            ExerciseLanguage = null;
        }

        public Exercise (int id, string exerciseName, string exerciseLanguage) {

        Id = id;
        ExerciseName = exerciseName;
        ExerciseLanguage = exerciseLanguage;
        ExerciseStudents = new List<Student>();

        }

        public Exercise(string exerciseName, string exerciseLanguage) {
        
            ExerciseName = exerciseName;
            ExerciseLanguage = exerciseLanguage;
            ExerciseStudents = new List<Student>();

        }

    public int Id { get; set; }

    [Required]
    public string ExerciseName { get; set; }

    [Required]
    public string ExerciseLanguage { get; set; }

    public List<Student> ExerciseStudents { get; set; }

    }
}

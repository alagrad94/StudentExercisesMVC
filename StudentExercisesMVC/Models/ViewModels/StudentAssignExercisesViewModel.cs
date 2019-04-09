using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models.ViewModels {

    public class StudentAssignExercisesViewModel {

        public Student Student { get; set; }
        public List<Exercise> ExercisesNotAssignedToStudent { get; set; }
        public List<Exercise> AssignedExercises { get; set; }
        public List<string> UnassignedExercises { get; set; }

        public List<SelectListItem> AssgignedExercisesOptions {
            get {
                if (AssignedExercises == null) {
                    return null;
                }

                return AssignedExercises.Select(ae => new SelectListItem {
                    Value = ae.Id.ToString(),
                    Text = ae.ExerciseName
                }).ToList();
            }
        }

        public List<SelectListItem> ExercisesNotAssignedToStudentOptions {
            get {
                if (ExercisesNotAssignedToStudent == null) {
                    return null;
                }

                return ExercisesNotAssignedToStudent.Select(ue => new SelectListItem {
                    Value = ue.Id.ToString(),
                    Text = ue.ExerciseName
                }).ToList();
            }
        }
    }
}

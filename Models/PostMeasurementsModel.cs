using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartACDeviceAPI.Models {

    public class PostMeasurementsModel{

        [Required]
        public List<Measurement> Measurements { get; set; }
    }

     public class PostMeasurementModelValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object o, ValidationContext context)
        {
            var model = o as PostMeasurementsModel;
            if (o != null)
            {
                if (model.Measurements.Count > 500)
                return new ValidationResult("Can not exceed 500");
            }

            return ValidationResult.Success;
        }
    }

}
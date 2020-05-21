using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SmartACDeviceAPI
{
    [MeasurementQueryModelValidator]
    public class MeasurementQueryModel
    {

        [DisplayFormat(DataFormatString = "{yyyy-MM-ddTHH:mm:ss}")]
        [BindProperty(SupportsGet = true, Name = "from")]
        public string FromDate { get; set; }

        [DisplayFormat(DataFormatString = "{yyyy-MM-ddTHH:mm:ssZ}")]
        [BindProperty(SupportsGet = true, Name = "to")]
        public string ToDate { get; set; }

    }

    public class MeasurementQueryModelValidator : ValidationAttribute
    {
        public MeasurementQueryModelValidator()
        {
        }

        protected override ValidationResult IsValid(object o, ValidationContext context)
        {
            Console.WriteLine(context.ObjectInstance.ToString());
            var model = o as MeasurementQueryModel;
            if (o != null)
            {
                if (!String.IsNullOrEmpty(model.FromDate) && String.IsNullOrEmpty(model.ToDate))
                {
                    return new ValidationResult("from parameter supplied. Must includefrom");
                }
                else if (!String.IsNullOrEmpty(model.ToDate) && String.IsNullOrEmpty(model.FromDate))
                {
                    return new ValidationResult("to parameter supplied. Must include from");
                }
            }

            return ValidationResult.Success;
        }
    }
}
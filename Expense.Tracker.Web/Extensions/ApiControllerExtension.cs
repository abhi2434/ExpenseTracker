using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;

public static class ApiControllerExtension
{
    public static IEnumerable<string> GetErrorsFromModelState(this ModelStateDictionary ModelState)
    {
        return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
    }
    public static IEnumerable<string> GetErrorsFromModelState(this System.Web.Mvc.ModelStateDictionary ModelState)
    {
        return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
    }

}

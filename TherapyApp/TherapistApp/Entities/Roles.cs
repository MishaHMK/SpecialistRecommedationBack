using Microsoft.AspNetCore.Mvc.Rendering;

namespace TherapyApp.Entities
{
    public class Roles
    {
        public static string Therapist = "Therapist";
        public static string Client = "Client";
        public static string Admin = "Admin";

        public static List<SelectListItem> GetRolesList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem {Value = Therapist, Text = Therapist},
                new SelectListItem {Value = Client, Text = Client},
                new SelectListItem {Value = Admin, Text = Admin}
            };
        }
    }
}

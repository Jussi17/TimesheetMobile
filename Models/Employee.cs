using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Android.Content.Res;
using Newtonsoft.Json;
using SQLite;
using Xamarin.Forms;

namespace TimesheetMobileApp.Models
{
    public class Employee
    {
        [PrimaryKey, AutoIncrement]
        public int IDEmployee { get; set; }
        [MaxLength(15)]
        public string FirstName { get; set; }
        [MaxLength(15)]
      
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool? Active { get; set; }
        public string ImageLink { get; set; }
    }
}

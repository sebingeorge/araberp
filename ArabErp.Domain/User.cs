﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class User
    {
        public int? UserId { get; set; }
        [Required]
        [Display(Name="User Name")]
        public string UserName { get; set; }
        [DataType(DataType.EmailAddress)]
        [Display(Name="Email")]
        public string UserEmail { get; set; }
        [DataType(DataType.Password)]
        [Display(Name="Password")]
        public string UserPassword { get; set; }
        [DataType(DataType.Password)]
        [Compare("UserPassword", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name="Confirm Password")]
        public string ConfirmPassword { get; set; }
        public string UserSalt { get; set; }
        public int? UserRole { get; set; }
        public List<ModuleVsUser> Module { get; set; }
    }
}

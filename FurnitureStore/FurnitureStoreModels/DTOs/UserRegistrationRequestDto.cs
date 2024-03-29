﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnitureStoreModels.DTOs
{
    //Esto es lo que le pedimos a alguien cuando se quiera registrar
    public class UserRegistrationRequestDto
    {
        [Required]//Esto devuelve badRequest
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

    }
}

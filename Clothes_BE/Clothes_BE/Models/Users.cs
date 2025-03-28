﻿using System.ComponentModel.DataAnnotations;

namespace Clothes_BE.Models
{
    public class Users
    {
        public int id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        [Phone]
        public string? phone { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public string? avatar { get; set; }
        public bool is_lock { get; set; }
        public string? refresh_token { get; set; }
        public DateTime? expiry_time { get; set; }
        public ICollection<Carts> carts { get; set; }
        public ICollection<Orders> orders { get; set; }

    }
}

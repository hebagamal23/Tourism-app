﻿using Tourism_project.Models;

namespace Tourism_project.Dtos.Home
{
    public class HotelDto
    {
        public int HotelId { get; set; }  
        public string Name { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }
        public int Stars { get; set; }

        public decimal PricePerNight { get; set; } 
        public string FirstImageUrl { get; set; }  
    }
}

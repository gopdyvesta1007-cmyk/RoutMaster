using System;

namespace RouteMaster.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Phone { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string PickupCity { get; set; }
        public string DeliveryCity { get; set; }
        public string CargoDescription { get; set; }
        public decimal? Weight { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CurrentStatus { get; set; }
    }

    public class OrderTracking
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RoutePoint
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string PointType { get; set; }
        public string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int Sequence { get; set; }
        public DateTime? EstimatedArrival { get; set; }
        public DateTime? ActualArrival { get; set; }
    }

    public class Vehicle
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; }
        public string Model { get; set; }
        public decimal Capacity { get; set; }
        public string Status { get; set; }
    }
}
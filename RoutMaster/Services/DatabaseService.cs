using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using RouteMaster.Models;
using RouteMaster.Database;

namespace RouteMaster.Services
{
    public class DatabaseService
    {
        public async Task<User> AuthenticateUser(string email, string password)
        {
            using (var connection = DbConnection.GetConnection())
            {
                var sql = @"SELECT u.*, r.Name as RoleName 
                           FROM Users u
                           JOIN Roles r ON u.RoleId = r.Id
                           WHERE u.Email = @Email AND u.PasswordHash = @Password";

                return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email, Password = password });
            }
        }

        public async Task<bool> RegisterUser(User user)
        {
            using (var connection = DbConnection.GetConnection())
            {
                var sql = @"INSERT INTO Users (FullName, Email, PasswordHash, Phone, RoleId) 
                           VALUES (@FullName, @Email, @PasswordHash, @Phone, @RoleId)";

                var result = await connection.ExecuteAsync(sql, user);
                return result > 0;
            }
        }

        public async Task<List<Order>> GetAllOrders(int? userId = null)
        {
            using (var connection = DbConnection.GetConnection())
            {
                var sql = @"SELECT o.*, u.FullName as ClientName 
                           FROM Orders o
                           LEFT JOIN Users u ON o.ClientId = u.Id
                           ORDER BY o.CreatedAt DESC";

                var result = await connection.QueryAsync<Order>(sql);
                return result.AsList();
            }
        }

        public async Task<List<Order>> GetMyOrders(int clientId)
        {
            using (var connection = DbConnection.GetConnection())
            {
                var sql = @"SELECT o.*, u.FullName as ClientName 
                           FROM Orders o
                           LEFT JOIN Users u ON o.ClientId = u.Id
                           WHERE o.ClientId = @ClientId
                           ORDER BY o.CreatedAt DESC";

                var result = await connection.QueryAsync<Order>(sql, new { ClientId = clientId });
                return result.AsList();
            }
        }

        public async Task<Order> GetOrderDetails(int orderId)
        {
            using (var connection = DbConnection.GetConnection())
            {
                var sql = @"SELECT o.*, u.FullName as ClientName 
                           FROM Orders o
                           LEFT JOIN Users u ON o.ClientId = u.Id
                           WHERE o.Id = @OrderId";

                return await connection.QueryFirstOrDefaultAsync<Order>(sql, new { OrderId = orderId });
            }
        }

        public async Task<List<OrderTracking>> GetOrderTracking(int orderId)
        {
            using (var connection = DbConnection.GetConnection())
            {
                var sql = @"SELECT * FROM OrderTracking WHERE OrderId = @OrderId ORDER BY CreatedAt DESC";
                var result = await connection.QueryAsync<OrderTracking>(sql, new { OrderId = orderId });
                return result.AsList();
            }
        }

        public async Task<int> GetOrderProgress(int orderId)
        {
            using (var connection = DbConnection.GetConnection())
            {
                var sql = @"SELECT CASE CurrentStatus
                            WHEN 'pending' THEN 0
                            WHEN 'packing' THEN 10
                            WHEN 'with_courier' THEN 25
                            WHEN 'in_transit' THEN 50
                            WHEN 'at_pickup_point' THEN 85
                            WHEN 'delivered' THEN 100
                            ELSE 0 END FROM Orders WHERE Id = @OrderId";

                return await connection.ExecuteScalarAsync<int>(sql, new { OrderId = orderId });
            }
        }
    }
}
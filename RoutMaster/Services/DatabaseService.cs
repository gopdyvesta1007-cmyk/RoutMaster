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

        public async Task<List<Order>> GetAllOrders()
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
                var sql = @"SELECT * FROM OrderTracking 
                           WHERE OrderId = @OrderId 
                           ORDER BY CreatedAt DESC";

                var result = await connection.QueryAsync<OrderTracking>(sql, new { OrderId = orderId });
                return result.AsList();
            }
        }

        public async Task<int> GetOrderProgress(int orderId)
        {
            using (var connection = DbConnection.GetConnection())
            {
                var sql = @"SELECT 
                            CASE CurrentStatus
                                WHEN 'pending' THEN 0
                                WHEN 'packing' THEN 10
                                WHEN 'with_courier' THEN 25
                                WHEN 'in_transit' THEN 50
                                WHEN 'at_pickup_point' THEN 85
                                WHEN 'delivered' THEN 100
                                ELSE 0
                            END
                            FROM Orders WHERE Id = @OrderId";

                var result = await connection.ExecuteScalarAsync<int>(sql, new { OrderId = orderId });
                return result;
            }
        }

        public async Task<List<Order>> GetFavoriteOrders(int userId)
        {
            using (var connection = DbConnection.GetConnection())
            {
                var sql = @"SELECT o.*, u.FullName as ClientName 
                           FROM Orders o
                           JOIN FavoriteOrders f ON o.Id = f.OrderId
                           LEFT JOIN Users u ON o.ClientId = u.Id
                           WHERE f.UserId = @UserId
                           ORDER BY f.CreatedAt DESC";

                var result = await connection.QueryAsync<Order>(sql, new { UserId = userId });
                return result.AsList();
            }
        }

        public async Task<bool> AddToFavorites(int userId, int orderId)
        {
            using (var connection = DbConnection.GetConnection())
            {
                try
                {
                    var sql = @"INSERT INTO FavoriteOrders (UserId, OrderId) VALUES (@UserId, @OrderId)";
                    var result = await connection.ExecuteAsync(sql, new { UserId = userId, OrderId = orderId });
                    return result > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AddToFavorites error: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<bool> RemoveFromFavorites(int userId, int orderId)
        {
            using (var connection = DbConnection.GetConnection())
            {
                var sql = @"DELETE FROM FavoriteOrders WHERE UserId = @UserId AND OrderId = @OrderId";
                var result = await connection.ExecuteAsync(sql, new { UserId = userId, OrderId = orderId });
                return result > 0;
            }
        }

        public async Task<int> GetActiveOrdersCount()
        {
            using (var connection = DbConnection.GetConnection())
            {
                var sql = @"SELECT COUNT(*) FROM Orders WHERE CurrentStatus NOT IN ('delivered', 'cancelled')";
                return await connection.ExecuteScalarAsync<int>(sql);
            }
        }

        public async Task<List<RoutePoint>> GetRoutePoints(int orderId)
        {
            using (var connection = DbConnection.GetConnection())
            {
                var sql = @"SELECT * FROM RoutePoints WHERE OrderId = @OrderId ORDER BY Sequence";
                var result = await connection.QueryAsync<RoutePoint>(sql, new { OrderId = orderId });
                return result.AsList();
            }
        }
    }
}
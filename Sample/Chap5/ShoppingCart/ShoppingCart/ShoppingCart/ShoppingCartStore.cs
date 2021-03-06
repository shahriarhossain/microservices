﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCartStore : IShoppingCartStore
    {
        private string connectionString =
            @"Data Source=TONYHUDSON\SQLEXPRESS;Initial Catalog=ShoppingCart;
            Integrated Security=True";

        private const string readItemsSql =
            @"select * from ShoppingCart, ShoppingCartItems
            where ShoppingCartItems.ShoppingCartId = ID
            and ShoppingCart.UserId=@UserId";

        private const string deleteAllForShoppingCartSql =
            @"delete item from ShoppingCartItems item
            inner join ShoppingCart cart on item.ShoppingCartId = cart.ID
            and cart.UserId=@UserId";

        private const string addAllForShoppingCartSql =
             @"insert into ShoppingCartItems
            (ShoppingCartId, ProductCatalogId, ProductName,
            ProductDescription, Amount, Currency)
            values
            (@ShoppingCartId, @ProductCatalogId, @ProductName,v
            @ProductDescription, @Amount, @Currency)";

        public async Task<ShoppingCart> Get(int userId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var items = await conn.QueryAsync<ShoppingCartItem>(readItemsSql, new { UserId = userId });
                return new ShoppingCart(userId, items);
            }
        }

        public async Task Save(ShoppingCart shoppingCart)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var tx = conn.BeginTransaction())
            {
                await conn.ExecuteAsync(deleteAllForShoppingCartSql, new { shoppingCart.UserId }).ConfigureAwait(false);
                await conn.ExecuteAsync(addAllForShoppingCartSql, shoppingCart.Items, tx).ConfigureAwait(false);
            }
        }
    }
}

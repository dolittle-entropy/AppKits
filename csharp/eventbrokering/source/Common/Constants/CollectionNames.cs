namespace Common.Constants
{
    /// <summary>
    /// All table names (collections in Mongo) should be named here as constants
    /// </summary>
    public static class CollectionNames
    {
        // Customer Prefix
        public const string CustomerPrefix = "Dolittle"; //TODO: Replace with a shorthand customer prefix

        // Ordering
        public const string Orders = nameof(Orders);
        public const string OrderLines = nameof(OrderLines);
        public const string OrderTypes = nameof(OrderTypes);

        // Products
        public const string Products = nameof(Products);

        // Warehousing
        public const string Warehouses = nameof(Warehouses);

        // Rejections
        public const string Rejections = nameof(Rejections);

        // Users Context
        public const string Users = nameof(Users);
    }
}

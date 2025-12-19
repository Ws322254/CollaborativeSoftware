namespace CollaborativeSoftware
{
    public static class Session
    {
        public static string CurrentUserEmail { get; set; }
        public static UserRole CurrentUserRole { get; set; }
        public static int CurrentUserId { get; set; }
    }
}

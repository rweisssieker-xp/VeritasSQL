namespace VeritasSQL.Core.Models;

/// <summary>
/// SQL Achievement - Gamification for data work!
/// Inspired by Steam/Xbox achievements
/// </summary>
public class Achievement
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "🏆";
    public AchievementCategory Category { get; set; }
    public AchievementRarity Rarity { get; set; }
    public int PointValue { get; set; }
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
    public int Progress { get; set; }
    public int Target { get; set; }
    public string ProgressText => $"{Progress}/{Target}";
    public double ProgressPercent => Target > 0 ? (double)Progress / Target * 100 : 0;
}

public enum AchievementCategory
{
    Beginner,       // First steps
    QueryMaster,    // Query-related achievements
    Explorer,       // Database exploration
    SpeedDemon,     // Performance-related
    Perfectionist,  // Quality-related
    Veteran,        // Long-term usage
    Social          // Sharing/collaboration
}

public enum AchievementRarity
{
    Common,         // Easy to get
    Uncommon,       // Moderate effort
    Rare,           // Significant effort
    Epic,           // Major milestone
    Legendary       // Exceptional achievement
}

/// <summary>
/// Static list of all available achievements
/// </summary>
public static class Achievements
{
    public static readonly List<Achievement> All = new()
    {
        // Beginner Achievements
        new Achievement
        {
            Id = "first_query",
            Name = "First Steps",
            Description = "Execute your first SQL query",
            Icon = "👶",
            Category = AchievementCategory.Beginner,
            Rarity = AchievementRarity.Common,
            PointValue = 10,
            Target = 1
        },
        new Achievement
        {
            Id = "first_connection",
            Name = "Connected!",
            Description = "Connect to your first database",
            Icon = "🔌",
            Category = AchievementCategory.Beginner,
            Rarity = AchievementRarity.Common,
            PointValue = 10,
            Target = 1
        },
        new Achievement
        {
            Id = "first_date",
            Name = "First Date",
            Description = "Use Database First Date to explore a new database",
            Icon = "💕",
            Category = AchievementCategory.Explorer,
            Rarity = AchievementRarity.Common,
            PointValue = 15,
            Target = 1
        },
        
        // Query Master Achievements
        new Achievement
        {
            Id = "query_10",
            Name = "Getting Started",
            Description = "Execute 10 queries",
            Icon = "📝",
            Category = AchievementCategory.QueryMaster,
            Rarity = AchievementRarity.Common,
            PointValue = 20,
            Target = 10
        },
        new Achievement
        {
            Id = "query_100",
            Name = "Query Apprentice",
            Description = "Execute 100 queries",
            Icon = "📊",
            Category = AchievementCategory.QueryMaster,
            Rarity = AchievementRarity.Uncommon,
            PointValue = 50,
            Target = 100
        },
        new Achievement
        {
            Id = "query_1000",
            Name = "Query Master",
            Description = "Execute 1,000 queries",
            Icon = "🎓",
            Category = AchievementCategory.QueryMaster,
            Rarity = AchievementRarity.Rare,
            PointValue = 100,
            Target = 1000
        },
        new Achievement
        {
            Id = "join_master",
            Name = "JOIN Master",
            Description = "Execute 50 queries with JOINs",
            Icon = "🔗",
            Category = AchievementCategory.QueryMaster,
            Rarity = AchievementRarity.Uncommon,
            PointValue = 40,
            Target = 50
        },
        
        // Explorer Achievements
        new Achievement
        {
            Id = "schema_explorer",
            Name = "Schema Explorer",
            Description = "Explore 10 different databases",
            Icon = "🗺️",
            Category = AchievementCategory.Explorer,
            Rarity = AchievementRarity.Uncommon,
            PointValue = 50,
            Target = 10
        },
        new Achievement
        {
            Id = "table_tourist",
            Name = "Table Tourist",
            Description = "Query 50 different tables",
            Icon = "🧳",
            Category = AchievementCategory.Explorer,
            Rarity = AchievementRarity.Uncommon,
            PointValue = 40,
            Target = 50
        },
        
        // Speed Demon Achievements
        new Achievement
        {
            Id = "speed_demon",
            Name = "Speed Demon",
            Description = "Execute a query in under 100ms",
            Icon = "⚡",
            Category = AchievementCategory.SpeedDemon,
            Rarity = AchievementRarity.Common,
            PointValue = 15,
            Target = 1
        },
        new Achievement
        {
            Id = "big_data",
            Name = "Big Data Handler",
            Description = "Successfully query 1 million+ rows",
            Icon = "🐘",
            Category = AchievementCategory.SpeedDemon,
            Rarity = AchievementRarity.Rare,
            PointValue = 75,
            Target = 1
        },
        
        // Perfectionist Achievements
        new Achievement
        {
            Id = "zero_errors_day",
            Name = "Perfect Day",
            Description = "Execute 20+ queries in a day with no errors",
            Icon = "✨",
            Category = AchievementCategory.Perfectionist,
            Rarity = AchievementRarity.Uncommon,
            PointValue = 35,
            Target = 20
        },
        new Achievement
        {
            Id = "zero_errors_week",
            Name = "Flawless Week",
            Description = "Go a full week with no query errors",
            Icon = "💎",
            Category = AchievementCategory.Perfectionist,
            Rarity = AchievementRarity.Rare,
            PointValue = 100,
            Target = 1
        },
        new Achievement
        {
            Id = "preview_user",
            Name = "Safety First",
            Description = "Use Query Preview 10 times before full execution",
            Icon = "🛡️",
            Category = AchievementCategory.Perfectionist,
            Rarity = AchievementRarity.Common,
            PointValue = 20,
            Target = 10
        },
        
        // Veteran Achievements
        new Achievement
        {
            Id = "week_streak",
            Name = "Weekly Warrior",
            Description = "Use VeritasSQL 7 days in a row",
            Icon = "📆",
            Category = AchievementCategory.Veteran,
            Rarity = AchievementRarity.Uncommon,
            PointValue = 50,
            Target = 7
        },
        new Achievement
        {
            Id = "month_streak",
            Name = "Monthly Master",
            Description = "Use VeritasSQL 30 days in a row",
            Icon = "🗓️",
            Category = AchievementCategory.Veteran,
            Rarity = AchievementRarity.Rare,
            PointValue = 150,
            Target = 30
        },
        new Achievement
        {
            Id = "power_user",
            Name = "Power User",
            Description = "Execute 100 queries in a single day",
            Icon = "🔥",
            Category = AchievementCategory.Veteran,
            Rarity = AchievementRarity.Epic,
            PointValue = 200,
            Target = 100
        },
        
        // Legendary Achievements
        new Achievement
        {
            Id = "legendary_analyst",
            Name = "Legendary Analyst",
            Description = "Reach 1,000 achievement points",
            Icon = "👑",
            Category = AchievementCategory.Veteran,
            Rarity = AchievementRarity.Legendary,
            PointValue = 500,
            Target = 1000
        }
    };
}

/// <summary>
/// User's achievement progress
/// </summary>
public class UserAchievements
{
    public int TotalPoints { get; set; }
    public int UnlockedCount { get; set; }
    public int TotalCount => Achievements.All.Count;
    public string Level => TotalPoints switch
    {
        < 50 => "Novice",
        < 150 => "Apprentice",
        < 300 => "Journeyman",
        < 500 => "Expert",
        < 1000 => "Master",
        _ => "Legendary"
    };
    public string LevelIcon => TotalPoints switch
    {
        < 50 => "🌱",
        < 150 => "🌿",
        < 300 => "🌳",
        < 500 => "⭐",
        < 1000 => "🌟",
        _ => "👑"
    };
    public List<Achievement> UnlockedAchievements { get; set; } = new();
    public List<Achievement> InProgressAchievements { get; set; } = new();
    public Achievement? LatestUnlocked { get; set; }
}


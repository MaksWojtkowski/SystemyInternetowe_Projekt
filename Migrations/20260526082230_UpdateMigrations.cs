using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArcadeProject.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Post_AspNetUsers_UserId",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_Thread_ThreadId",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_Thread_AspNetUsers_UserId",
                table: "Thread");

            migrationBuilder.DropForeignKey(
                name: "FK_Thread_Games_GameId",
                table: "Thread");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievement_Achievements_AchievementId",
                table: "UserAchievement");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievement_AspNetUsers_UserId",
                table: "UserAchievement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAchievement",
                table: "UserAchievement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Thread",
                table: "Thread");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Post",
                table: "Post");

            migrationBuilder.RenameTable(
                name: "UserAchievement",
                newName: "UserAchievements");

            migrationBuilder.RenameTable(
                name: "Thread",
                newName: "Threads");

            migrationBuilder.RenameTable(
                name: "Post",
                newName: "Posts");

            migrationBuilder.RenameIndex(
                name: "IX_UserAchievement_UserId_AchievementId",
                table: "UserAchievements",
                newName: "IX_UserAchievements_UserId_AchievementId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAchievement_AchievementId",
                table: "UserAchievements",
                newName: "IX_UserAchievements_AchievementId");

            migrationBuilder.RenameIndex(
                name: "IX_Thread_UserId",
                table: "Threads",
                newName: "IX_Threads_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Thread_GameId",
                table: "Threads",
                newName: "IX_Threads_GameId");

            migrationBuilder.RenameIndex(
                name: "IX_Post_UserId",
                table: "Posts",
                newName: "IX_Posts_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Post_ThreadId",
                table: "Posts",
                newName: "IX_Posts_ThreadId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAchievements",
                table: "UserAchievements",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Threads",
                table: "Threads",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Posts",
                table: "Posts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_UserId",
                table: "Posts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Threads_ThreadId",
                table: "Posts",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Threads_AspNetUsers_UserId",
                table: "Threads",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Threads_Games_GameId",
                table: "Threads",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievements_Achievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId",
                principalTable: "Achievements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievements_AspNetUsers_UserId",
                table: "UserAchievements",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_UserId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Threads_ThreadId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Threads_AspNetUsers_UserId",
                table: "Threads");

            migrationBuilder.DropForeignKey(
                name: "FK_Threads_Games_GameId",
                table: "Threads");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievements_Achievements_AchievementId",
                table: "UserAchievements");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievements_AspNetUsers_UserId",
                table: "UserAchievements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAchievements",
                table: "UserAchievements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Threads",
                table: "Threads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Posts",
                table: "Posts");

            migrationBuilder.RenameTable(
                name: "UserAchievements",
                newName: "UserAchievement");

            migrationBuilder.RenameTable(
                name: "Threads",
                newName: "Thread");

            migrationBuilder.RenameTable(
                name: "Posts",
                newName: "Post");

            migrationBuilder.RenameIndex(
                name: "IX_UserAchievements_UserId_AchievementId",
                table: "UserAchievement",
                newName: "IX_UserAchievement_UserId_AchievementId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAchievements_AchievementId",
                table: "UserAchievement",
                newName: "IX_UserAchievement_AchievementId");

            migrationBuilder.RenameIndex(
                name: "IX_Threads_UserId",
                table: "Thread",
                newName: "IX_Thread_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Threads_GameId",
                table: "Thread",
                newName: "IX_Thread_GameId");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_UserId",
                table: "Post",
                newName: "IX_Post_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_ThreadId",
                table: "Post",
                newName: "IX_Post_ThreadId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAchievement",
                table: "UserAchievement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Thread",
                table: "Thread",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Post",
                table: "Post",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Post_AspNetUsers_UserId",
                table: "Post",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Post_Thread_ThreadId",
                table: "Post",
                column: "ThreadId",
                principalTable: "Thread",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Thread_AspNetUsers_UserId",
                table: "Thread",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Thread_Games_GameId",
                table: "Thread",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievement_Achievements_AchievementId",
                table: "UserAchievement",
                column: "AchievementId",
                principalTable: "Achievements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievement_AspNetUsers_UserId",
                table: "UserAchievement",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

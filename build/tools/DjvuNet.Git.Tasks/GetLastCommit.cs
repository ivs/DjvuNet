﻿using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DjvuNet.Git.Tasks
{
    public class GetLastCommit : Task
    {
        /// <summary>
        /// Git repo root directory
        /// </summary>
        [Required]
        public string RepoRoot { get; set; }

        /// <summary>
        /// Last commit hash
        /// </summary>
        [Output]
        public string CommitHash { get; set; }

        /// <summary>
        /// UTC DateTime when commit was created.
        /// </summary>
        [Output]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Name of the commit author
        /// </summary>
        [Output]
        public string Author { get; set; }

        /// <summary>
        /// Email of commit author
        /// </summary>
        [Output]
        public string AuthorEmail { get; set; }

        /// <summary>
        /// Short commit message - first line
        /// </summary>
        [Output]
        public string MessageShort { get; set; }

        /// <summary>
        /// Commit message
        /// </summary>
        [Output]
        public string Message { get; set; }

        public override bool Execute()
        {
            if (String.IsNullOrWhiteSpace(RepoRoot))
            {
                Log.LogError($"Invalid path string: {RepoRoot}");
                return false;
            }

            if (!Directory.Exists(RepoRoot))
            {
                Log.LogError($"Path does not exist: {RepoRoot}");
                return false;
            }

            try
            {
                // This prevents failed builds
                Commit commit = null;

                try
                {
                    Repository repo = new Repository(RepoRoot);
                    var commits = repo.Commits;
                    commit = commits.FirstOrDefault();
                }
                catch(LibGit2SharpException)
                {
                }

                if (commit != null)
                {
                    CommitHash = commit.Sha;
                    Author = commit.Author.Name;
                    AuthorEmail = commit.Author.Email;
                    DateTime = commit.Author.When.UtcDateTime;
                    MessageShort = commit.MessageShort;
                    Message = commit.Message;
                }
                else
                {
                    CommitHash = "No hash available";
                    Author = String.Empty;
                    AuthorEmail = String.Empty;
                    DateTime = DateTime.UtcNow;
                    MessageShort = "Failed to retrieve git repo data. This could be out of repo build.";
                    Message = MessageShort;
                }
            }
            catch(Exception ex)
            {
                Log.LogError(ex.ToString());
                Log.LogErrorFromException(ex, true);
            }

            return !Log.HasLoggedErrors;
        }
    }
}

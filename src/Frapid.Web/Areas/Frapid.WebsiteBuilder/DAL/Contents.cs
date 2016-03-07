﻿using System.Collections.Generic;
using System.Linq;
using Frapid.ApplicationState.Cache;
using Frapid.Configuration;
using Frapid.DataAccess;
using Frapid.WebsiteBuilder.DTO;

namespace Frapid.WebsiteBuilder.DAL
{
    public class Contents
    {
        public static IEnumerable<Content> GetContents()
        {
            using (var db = DbProvider.Get(ConnectionString.GetConnectionString(AppUsers.GetTenant())).GetDatabase())
            {
                return db.FetchBy<Content>(sql => sql.Where(c => c.IsHomepage));
            }
        }

        public static Content Get(int contentId)
        {
            using (var db = DbProvider.Get(ConnectionString.GetConnectionString(AppUsers.GetTenant())).GetDatabase())
            {
                return db.FetchBy<Content>(sql => sql
                    .Where(c => c.ContentId == contentId))
                    .FirstOrDefault();
            }
        }

        public static PublishedContentView GetPublished(string tenant, string categoryAlias, string alias, bool isBlog)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                return GetDefault();
            }

            using (var db = DbProvider.Get(ConnectionString.GetConnectionString(tenant)).GetDatabase())
            {
                return db.FetchBy<PublishedContentView>(sql => sql
                    .Where(c => c.Alias.ToLower().Equals(alias.ToLower()) 
                            && c.CategoryAlias.ToLower().Equals(categoryAlias.ToLower())
                            && c.IsBlog == isBlog
                            ))
                    .FirstOrDefault();
            }
        }

        public static IEnumerable<PublishedContentView> GetBlogContents()
        {
            using (var db = DbProvider.Get(ConnectionString.GetConnectionString(AppUsers.GetTenant())).GetDatabase())
            {
                return db.FetchBy<PublishedContentView>(sql => sql.Where(x=>x.IsBlog)).Take(10);
            }
        }

        public static PublishedContentView GetDefault()
        {
            using (var db = DbProvider.Get(ConnectionString.GetConnectionString(AppUsers.GetTenant())).GetDatabase())
            {
                return
                    db.FetchBy<PublishedContentView>(sql => sql.Where(c => c.IsHomepage == true).Limit(1))
                        .FirstOrDefault();
            }
        }

        public static void AddHit(string database, int contentId)
        {
            const string sql = "UPDATE website.contents SET hits = COALESCE(website.contents.hits, 0) + 1 WHERE website.contents.content_id=@0;";
            Factory.NonQuery(database, sql, contentId);
        }
    }
}
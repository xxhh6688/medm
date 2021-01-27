using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MatchSystem.Authentication;

namespace MatchSystem.Authentication
{
    using System.Data.Entity.Migrations.Model;
    using System.Runtime.Serialization;
    using DocumentFormat.OpenXml.Wordprocessing;

    public class ValidationContainer
    {
        static ValidationContainer instance = new ValidationContainer();

        List<Tuple<string, string, IAuthSpecification>> items;

        List<Validation> validations;

        public ValidationContainer()
        {
            items = new List<Tuple<string, string, IAuthSpecification>>();
            validations = new List<Validation>();

            validations.Add(new Validation("user")
            {
                AddOneSpecification = new UserTypeOf(8 | 16),
                UpdateSpecification = new Permission<Models.user>(
                    (resource, input, currentusr) =>
                        (currentusr.Id == input.Id) &&          // owner
                        (currentusr.Type == input.Type) &&      // type canont be modified
                        ((currentusr.Type & 1) == 1) ||
                        ((currentusr.Type & 2) == 2)
                        )                  // only expert can update itself
                .Or(new UserTypeOf(8 | 16)),                    // manager and admin have permission
                RemoveSpecification = new Admin(),
                GetOneSpecification = new Permission<Models.user>(
                    (resource, input, currentusr) =>
                        (currentusr.Id == resource.Id))         // owner
                .Or(new UserTypeOf(8 | 16)),
                GetListSpecification = new UserTypeOf(8 | 16)
            });

            validations.Add(new Validation("user_security")
            {
                AddOneSpecification = new Admin(),
                UpdateSpecification = new Admin(),
                RemoveSpecification = new Admin(),
                GetOneSpecification = new Admin(),
                GetListSpecification = new Admin()
            });

            validations.Add(new Validation("token")
            {
                AddOneSpecification = new Ban(),
                UpdateSpecification = new Ban(),
                RemoveSpecification = new Ban(),
                GetOneSpecification = new Ban(),
                GetListSpecification = new Ban()
            });

            validations.Add(new Validation("message")
            {
                AddOneSpecification = new UserTypeOf(8 | 16),
                UpdateSpecification = new UserTypeOf(8 | 16),
                RemoveSpecification = new UserTypeOf(8 | 16),
                GetOneSpecification = new Allow(),
                GetListSpecification = new Allow()
            });

            validations.Add(new Validation("message_user_ref")
            {
                AddOneSpecification = new UserTypeOf(8 | 16),
                UpdateSpecification = new UserTypeOf(8 | 16),
                RemoveSpecification = new UserTypeOf(8 | 16)
                    .Or(new Permission<Models.message_user_ref>(
                        (oldOne, newOne, usr) => oldOne.UserId == usr.Id
                    )),
                GetOneSpecification = new UserTypeOf(8 | 16)
                    .Or(new Permission<Models.message_user_ref>(
                        (oldOne, newOne, usr) => oldOne.UserId == usr.Id
                    )),
                GetListSpecification = new UserTypeOf(8 | 16)
            });

            validations.Add(new Validation("expert")
            {
                AddOneSpecification = new UserTypeOf(8 | 16),
                UpdateSpecification = new Permission<Models.expert>(
                    (old, update, user) => update.UserId == user.Id
                ).Or(new UserTypeOf(8 | 16)),
                RemoveSpecification = new UserTypeOf(8 | 16),            //just remove user
                GetOneSpecification = new Permission<Models.expert>(
                    (old, update, user) => old.UserId == user.Id
                ).Or(new UserTypeOf(8 | 16)),
                GetListSpecification = new UserTypeOf(8 | 16)
            });

            validations.Add(new Validation("majorcontact")
            {
                AddOneSpecification = new UserTypeOf(8 | 16),
                UpdateSpecification = new UserTypeOf(8 | 16),
                RemoveSpecification = new UserTypeOf(8 | 16),
                GetOneSpecification = new Permission<Models.majorcontact>(
                    (old, update, user) => old.UserId == user.Id
                ).Or(new UserTypeOf(8 | 16)),
                GetListSpecification = new Allow()
            });

            validations.Add(new Validation("major")
            {
                AddOneSpecification = new UserTypeOf(8 | 16),
                UpdateSpecification = new UserTypeOf(8 | 16),
                RemoveSpecification = new UserTypeOf(8 | 16),
                GetOneSpecification = new Allow(),
                GetListSpecification = new Allow()
            });

            validations.Add(new Validation("school")
            {
                AddOneSpecification = new UserTypeOf(8 | 16),
                UpdateSpecification = new UserTypeOf(8 | 16),
                RemoveSpecification = new UserTypeOf(8 | 16),
                GetOneSpecification = new Allow(),
                GetListSpecification = new Allow()
            });

            /*validations.Add(new Validation("paper_review_expert")
            {
                AddOneSpecification = new UserTypeOf(8 | 16),
                UpdateSpecification = new UserTypeOf(16)
                    .Or(new Permission<Models.paper_review_expert>(
                        (old, update, user) =>
                        {
                            var pre = update.PaperId == old.PaperId
                                      && update.ExpertId == old.ExpertId
                                      && update.ExpertId == user.Id
                                      && (user.Type & (3|8)) != 0;
                            if (!pre) return false;
                            var res = false;
                            var match = old.paper.match;
                            switch (old.StatusType)
                            {
                                case 1:
                                    res = DateTime.UtcNow > match.ReviewStartTime
                                          && DateTime.UtcNow < match.Review1EndTime;
                                    break;
                                case 2:
                                    res = DateTime.UtcNow > match.Review1StartTime
                                          && DateTime.UtcNow < match.Review1EndTime;
                                    break;
                                case 3:
                                    res = DateTime.UtcNow > match.Review2StartTime
                                          && DateTime.UtcNow < match.Review2EndTime;
                                    break;
                                default: break;
                            }
                            return res;
                        }
                    )),
                RemoveSpecification = new UserTypeOf(8 | 16),
                GetOneSpecification = new UserTypeOf(8 | 16)
                    .Or(new Permission<Models.paper_review_expert>(
                        (old, ignore, user) => old.ExpertId == user.Id
                        )),
                GetListSpecification = new UserTypeOf(8 | 16)
            });*/

            validations.Add(new Validation("paper")
            {
                AddOneSpecification = new UserTypeOf(8 | 16)
                    .Or(new Permission<Models.paper>(
                        (ignore, add, user) =>
                        {
                            //var pre = (user.Type & 4) != 0;
                            //if (!pre) return false;
                            // every body could add paper

                            var ctx = Utilities.GetDBContext();
                            var match = ctx.match.Find(add.MatchId);
                            if (match == null) return false;

                            return DateTime.UtcNow < match.RegisterEndTime
                                && DateTime.UtcNow > match.RegisterStartTime;
                        })),
                UpdateSpecification = new UserTypeOf(8 | 16)
                    .Or(new Permission<Models.paper>(
                        (old, update, user) =>
                        {
                            // but need owner update
                            var pre = update.CreateBy == user.Id;
                            if (!pre) return false;

                            var ctx = Utilities.GetDBContext();
                            var match = ctx.match.Find(update.MatchId);
                            if (match == null) return false;

                            return DateTime.UtcNow < match.RegisterEndTime
                                   && DateTime.UtcNow > match.RegisterStartTime;
                        })),
                RemoveSpecification = new UserTypeOf(8 | 16)
                    .Or(new Permission<Models.paper>(
                        (old, ignore, user) =>
                        {
                            var pre = old.CreateBy == user.Id;
                            if (!pre) return false;

                            var ctx = Utilities.GetDBContext();
                            var match = ctx.match.Find(old.MatchId);
                            if (match == null) return false;

                            return DateTime.UtcNow < match.RegisterEndTime
                                   && DateTime.UtcNow > match.RegisterStartTime;
                        })),
                GetOneSpecification = new Allow(),
                GetListSpecification = new Allow()
            });

            validations.Add(new Validation("question")
            {
                AddOneSpecification = new UserTypeOf(8 | 16),
                UpdateSpecification = new UserTypeOf(8 | 16),
                RemoveSpecification = new UserTypeOf(8 | 16),
                GetOneSpecification = new Allow(),
                GetListSpecification = new Allow()
            });

            validations.Add(new Validation("match_question_ref")
            {
                AddOneSpecification = new UserTypeOf(8 | 16),
                UpdateSpecification = new UserTypeOf(8 | 16),
                RemoveSpecification = new UserTypeOf(8 | 16),
                GetOneSpecification = new Allow(),
                GetListSpecification = new Allow()
            });

            validations.Add(new Validation("match")
            {
                AddOneSpecification = new UserTypeOf(8 | 16),
                UpdateSpecification = new UserTypeOf(8 | 16),
                RemoveSpecification = new UserTypeOf(8 | 16),
                GetOneSpecification = new Allow(),
                GetListSpecification = new Allow()
            });

            validations.Add(new Validation("match_file_ref")
            {
                AddOneSpecification = new UserTypeOf(8 | 16),
                UpdateSpecification = new UserTypeOf(8 | 16),
                RemoveSpecification = new UserTypeOf(8 | 16),
                GetOneSpecification = new Allow(),
                GetListSpecification = new Allow()
            });

            validations.Add(new Validation("file")
            {
                AddOneSpecification = new Ban(),     // support by file controller
                UpdateSpecification = new Ban(),
                RemoveSpecification = new Ban(),
                GetOneSpecification = new Allow(),
                GetListSpecification = new UserTypeOf(8 | 16)
            });

            validations.Add(new Validation("paper_file_ref")
            {
                AddOneSpecification = new UserTypeOf(8 | 16)
                    .Or(new Permission<Models.paper_file_ref>(
                        (ignore, add, user) =>
                        {
                            var ctx = Utilities.GetDBContext();
                            var paper = ctx.paper.Find(add.PaperId);
                            if (paper == null) return false;

                            var pre = paper.CreateBy == user.Id;
                            if (!pre) return false;

                            var match = ctx.match.Find(paper.MatchId);
                            if (match == null) return false;

                            return DateTime.UtcNow < match.RegisterEndTime
                                   && DateTime.UtcNow > match.RegisterStartTime;
                        })),
                UpdateSpecification = new Ban(),
                RemoveSpecification = new UserTypeOf(8 | 16)
                    .Or(new Permission<Models.paper_file_ref>(
                        (old, ignore, user) =>
                        {
                            var ctx = Utilities.GetDBContext();
                            var paper = ctx.paper.Find(old.PaperId);
                            if (paper == null) return false;

                            var pre = (user.Type & 4) != 0 && paper.CreateBy == user.Id;
                            if (!pre) return false;

                            var match = ctx.match.Find(paper.MatchId);
                            if (match == null) return false;

                            return DateTime.UtcNow < match.RegisterEndTime
                                   && DateTime.UtcNow > match.RegisterStartTime;
                        })),
                GetOneSpecification = new Allow(),
                GetListSpecification = new UserTypeOf(8 | 16)
            });

            validations.Add(new Validation("article_file_ref")
            {
                AddOneSpecification = new UserTypeOf(8 | 16),
                UpdateSpecification = new UserTypeOf(8 | 16),
                RemoveSpecification = new UserTypeOf(8 | 16),
                GetOneSpecification = new Allow(),
                GetListSpecification = new Allow()
            });

            validations.Add(new Validation("article")
            {
                AddOneSpecification = new UserTypeOf(8 | 16),
                UpdateSpecification = new UserTypeOf(8 | 16),
                RemoveSpecification = new UserTypeOf(8 | 16),
                GetOneSpecification = new Allow(),
                GetListSpecification = new Allow()
            });

            validations.Add(new Validation("tttttttt")
            {
                AddOneSpecification = new Allow(),
                UpdateSpecification = new Allow(),
                RemoveSpecification = new Allow(),
                GetOneSpecification = new Allow(),
                GetListSpecification = new Allow()
            });
        }

        public static ValidationContainer Instance { get { return instance; } }

        public bool Verify(AuthorithenContext context)
        {
            foreach (var item in this.validations)
            {
                if (item.IsMatchEntityName(context.EntityTypeName))
                {
                    if (!item.IsValid(context))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
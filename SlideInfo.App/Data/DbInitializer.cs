using System;
using System.Linq;
using System.Text;
using SlideInfo.App.Constants;
using SlideInfo.App.Helpers;
using SlideInfo.App.Models;
using SlideInfo.Core;

namespace SlideInfo.App.Data
{
    public static class DbInitializer
    {
        public static void Initialize(SlideInfoDbContext context)
        {
            context.Database.EnsureCreated();
            InsertSlides(context);
            InsertUsers(context);
            InsertComments(context);
            InsertMessages(context);
            context.SaveChanges();
        }

        private static void InsertSlides(SlideInfoDbContext context)
        {
            if (context.Slides.Any())
                return;

            var dirs = FileFilter.Filter(AppDirectories.SlideStorage, FileFilter.OpenSlideExtensions);

            foreach (var path in dirs)
            {
                var osr = new OpenSlide(path);

                var slide = new Slide(osr);
                context.Slides.Add(slide);
                context.SaveChanges();

                var properties = osr.ReadProperties();

                foreach (var slideProp in properties)
                {
                    var property = new Property(slide.Id, slideProp.Key, slideProp.Value);
                    context.Properties.Add(property);
                }
            }
            context.SaveChanges();
        }

        private static void InsertUsers(SlideInfoDbContext context)
        {
            if (context.AppUsers.Any())
                return;

            var user1 = new AppUser()
            {
                FirstMidName = "Marcin",
                LastName = "Kowalski",
                Email = "mail@interia.eu",
                UserName = "mail@interia.eu",
                LastActive = DateTime.Now,
                EmailConfirmed = true,
            };

            var user2 = new AppUser()
            {
                FirstMidName = "Jan",
                LastName = "Kowalczuk",
                Email = "mail@onet.pl",
                UserName = "mail@onet.pl",
                LastActive = DateTime.Now,
                EmailConfirmed = true,
            };

            context.AppUsers.Add(user1);
            context.AppUsers.Add(user2);
            context.SaveChanges();
        }

        private static void InsertComments(SlideInfoDbContext context)
        {
            if (context.Comments.Count() > 50)
                return;

            var userCount = context.Users.Count();
            var slidesCount = context.Slides.Count();

            var random = new Random();

            for (var i = 0; i < 50; i++)
            {
                var usersToSkip = random.Next(0, userCount);
                var randomAppUser = context.Users.OrderBy(r => Guid.NewGuid()).Skip(usersToSkip).Take(1)
                    .FirstOrDefault();

                var slidesToSkip = random.Next(0, slidesCount);
                var randomSlide = context.Slides.OrderBy(r => Guid.NewGuid()).Skip(slidesToSkip).Take(1)
                    .FirstOrDefault();

                var comment = new Comment
                {
                    Slide = randomSlide,
                    SlideId = randomSlide.Id,
                    AppUser = randomAppUser,
                    AppUserId = randomAppUser.Id,
                    Date = DateTime.Now.AddMinutes(random.Next(0, i)),
                    Text = $"Example comment {i}"
                };
                context.Comments.Add(comment);
            }
            context.SaveChanges();
        }

        private static void InsertMessages(SlideInfoDbContext context)
        {
            if (context.Messages.Any())
                return;

            for (var i = 0; i < 31; ++i)
            {
                var toId = context.Users.OrderBy(u => Guid.NewGuid()).First().Id;
                var fromId = context.Users.First(u => u.Id != toId).Id;
                var subject = Conversation.GenerateConversationSubject(fromId, toId);
                var message = new Message
                {
                    Content = "Very important content of " + i + " very important message to display on user chat",
                    FromId = fromId,
                    ToId = toId,
                    Subject = subject,
                    DateSent = DateTime.Now.Subtract(TimeSpan.FromMinutes(0.5 * i))
                };
                context.Messages.Add(message);
                if (context.Conversations.Find(subject) == null)
                {
                    context.Conversations.Add(new Conversation { Subject = subject });
                }
            }

            context.SaveChanges();
        }

        public static void UpdateSlides(SlideInfoDbContext context)
        {
            context.Database.EnsureCreated();

            var dirs = FileFilter.Filter(AppDirectories.SlideStorage, FileFilter.OpenSlideExtensions);

            foreach (var path in dirs)
            {
                var osr = new OpenSlide(path);

                var newSlide = new Slide(osr);

                var existingSlide = context.Slides.FirstOrDefault(s => s.FilePath == path);

                if (existingSlide != null)
                {
                    newSlide.Id = existingSlide.Id;
                    context.Entry(existingSlide).CurrentValues.SetValues(newSlide);
                }
                else
                {
                    context.Add(newSlide);

                    var properties = osr.ReadProperties();
                    foreach (var slideProp in properties)
                    {
                        var property = new Property(newSlide.Id, slideProp.Key, slideProp.Value);
                        context.Add(property);
                    }
                }
            }
            context.SaveChanges();
        }
    }
}

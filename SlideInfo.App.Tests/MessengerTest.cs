using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlideInfo.App.Data;
using SlideInfo.App.Models;
using SlideInfo.App.Controllers;
using SlideInfo.Helpers;

namespace SlideInfo.App.Tests
{
    [TestClass]
    public class MessageTest
    {
        [TestMethod]
        public void MessageShouldNotBeRead()
        {
            var message = new Message { DateSent = DateTime.Now };

            Assert.AreEqual(false, message.IsRead());
        }

        [TestMethod]
        public void MessageShouldBeRead()
        {
            var message = new Message { DateSent = DateTime.Now, DateRead = DateTime.Now.AddMilliseconds(22) };

            Assert.AreEqual(true, message.IsRead());
        }
    }

    [TestClass]
    public class SubjectGeneratorTest
    {
        [TestMethod]
        public void ShouldGenerateSortedSubject()
        {
            var userIds = new[] { "123-kkkk", "999-kkkks-qwqw", "453-oooo-popo" };

            var subject = Conversation.GenerateConversationSubject(userIds);
            Assert.AreEqual("123-453-999-", subject);
        }
    }


    [TestClass]
    public class ConversationHelperTest
    {
        [TestMethod]
        public void ShouldGetCorrectConversationUsersList()
        {
            var messages = new[]
               {
                new Message {FromId = "1"}, new Message {FromId = "2"}, new Message {FromId = "1"},
                new Message {FromId = "5"}, new Message {FromId = "1"}, new Message {FromId = "2"}
            }.AsEnumerable();

            var users = Conversation.GetConversationUserNames(messages).ToArray();

            CollectionAssert.AreEqual(new[] { "1", "2", "5" }, users);
        }
    }

    [TestClass]
    public class StringExtensionHelperTest
    {
        [TestMethod]
        public void ShouldContainString()
        {
            var toCheck = new[] { "ok", "ss", "qq" };
            const string str = "qwertyss";
            Assert.AreEqual(true, str.ContainsAny(toCheck));
        }

        [TestMethod]
        public void ShouldNotContainString()
        {
            var toCheck = new[] { "ok", "ss", "qq" };
            const string str = "qwertyaa";
            Assert.AreEqual(false, str.ContainsAny(toCheck));
        }
    }

}

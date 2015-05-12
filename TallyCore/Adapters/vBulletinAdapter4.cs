﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NetTally.Adapters
{
    public class vBulletinAdapter4 : IForumAdapter
    {
        public vBulletinAdapter4()
        {

        }

        public vBulletinAdapter4(string site)
        {
            ForumUrl = site;
            ThreadsUrl = site;
            PostsUrl = site;
        }

        protected virtual string ForumUrl { get; }
        protected virtual string ThreadsUrl { get; }
        protected virtual string PostsUrl { get; }




        // Bad characters we want to remove
        // \u200b = Zero width space (8203 decimal/html).  Trim() does not remove this character.
        readonly Regex badCharactersRegex = new Regex("\u200b");
        // Extract color attributes from span style.
        readonly Regex spanColorRegex = new Regex(@"\bcolor\s*:\s*(?<color>\w+)", RegexOptions.IgnoreCase);


        #region Public interface functions

        // Functions for constructing URLs

        public string GetPageUrl(string threadName, int page)
        {
            if (threadName == null)
                throw new ArgumentNullException(nameof(threadName));
            if (threadName == string.Empty)
                throw new ArgumentOutOfRangeException(nameof(threadName));

            if (page == 1)
                return threadName;

            string trailSlash = "";
            if (!threadName.EndsWith("/"))
                trailSlash = "/";

            return threadName + trailSlash + "page" + page.ToString();
        }

        public string GetPostUrlFromId(string threadName, string postId)
        {
            if (threadName == null)
                throw new ArgumentNullException(nameof(threadName));
            if (threadName == string.Empty)
                throw new ArgumentOutOfRangeException(nameof(threadName));
            if (postId == null)
                throw new ArgumentNullException(nameof(postId));
            if (postId == string.Empty)
                throw new ArgumentOutOfRangeException(nameof(postId));

            // http://www.fandompost.com/oldforums/showthread.php?39239-Yurikuma-Arashi-Discussion-Thread&p=288335#post288335

            return threadName + "&p=" + postId + "#post" + postId;
        }

        public string GetRelativeUrl(string relative) => ForumUrl + relative;

        /// <summary>
        /// Get the title of the web page.
        /// </summary>
        /// <param name="page">The page to search.</param>
        /// <returns>Returns the title of the page.</returns>
        public string GetPageTitle(HtmlDocument page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var title = page.DocumentNode.Element("html")?.Element("head")?.Element("title")?.InnerText;

            if (title == null)
                return string.Empty;

            return CleanupPostString(title);
        }

        /// <summary>
        /// Check if the name of the thread is valid for inserting into a URL.
        /// </summary>
        /// <param name="name">The name of the quest/thread.</param>
        /// <returns>Returns true if the name is valid.</returns>
        public bool IsValidThreadName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            // vBulletin thread name always starts with showthread.php
            Regex validateQuestNameForUrl = new Regex(@"^showthread.php");
            return validateQuestNameForUrl.Match(name).Success;
        }

        /// <summary>
        /// Get the author of the thread.
        /// </summary>
        /// <param name="page">The page to search.</param>
        /// <returns>Returns the thread author.</returns>
        public string GetAuthorOfThread(HtmlDocument page)
        {
            // vBulletin does not provide thread author information
            return string.Empty;
        }

        /// <summary>
        /// Calculate the page number that corresponds to the post number given.
        /// </summary>
        /// <param name="post">Post number.</param>
        /// <returns>Page number.</returns>
        public int GetPageNumberFromPostNumber(int postNumber) => ((postNumber - 1) / 20) + 1;

        /// <summary>
        /// Get the last page number of the thread, based on info available
        /// from the given page.
        /// </summary>
        /// <param name="page">The page to search.</param>
        /// <returns>Returns the last page number of the thread.</returns>
        public int GetLastPageNumberOfThread(HtmlDocument page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));


            var paginationTop = page.DocumentNode.Descendants("div").FirstOrDefault(a => a.Id == "pagination_top");

            var paginationForm = paginationTop.Element("form");

            // If there is no form, that means there's only one page in the thread.
            if (paginationForm == null)
                return 1;

            var lastPageSpan = paginationTop.Descendants("span").LastOrDefault(a => a.GetAttributeValue("class", "") == "first_last");

            if (lastPageSpan != null)
            {
                var lastPageLink = lastPageSpan.Element("a")?.GetAttributeValue("href", "");

                if (lastPageLink != null && lastPageLink != string.Empty)
                {
                    Regex lastPageRegex = new Regex(@"/page(?<lastPage>\d+)");
                    Match m = lastPageRegex.Match(lastPageLink);
                    if (m.Success)
                    {
                        int lastPage = 0;
                        if (int.TryParse(m.Groups["lastPage"].Value, out lastPage))
                            return lastPage;
                    }
                }
            }

            throw new InvalidOperationException("Unable to get the last page number of the thread.");
        }

        /// <summary>
        /// Given a page, return a list of posts on the page.
        /// </summary>
        /// <param name="page">The page to search.</param>
        /// <returns>Returns an enumerable list of posts.</returns>
        public IEnumerable<HtmlNode> GetPostsFromPage(HtmlDocument page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var postList = GetPostListFromPage(page);
            return GetPostsFromList(postList);
        }

        /// <summary>
        /// Get the ID string of the provided post (the portion that can be used in a URL).
        /// </summary>
        /// <param name="post">The post to query.</param>
        /// <returns>Returns the portion of the ID that can be inserted into a URL
        /// to reach this post.</returns>
        public string GetIdOfPost(HtmlNode post)
        {
            if (post == null)
                throw new ArgumentNullException(nameof(post));

            string postIdString = post.Id;
            if (postIdString.StartsWith("post_"))
                postIdString = postIdString.Substring("post_".Length);

            return postIdString;
        }

        /// <summary>
        /// This gets the sequential post number of a given post message.
        /// </summary>
        /// <param name="post">The post to inspect.</param>
        /// <returns>Returns the post number that's found.</returns>
        public int GetPostNumberOfPost(HtmlNode post)
        {
            if (post == null)
                throw new ArgumentNullException(nameof(post));

            var postHead = post.Elements("div").FirstOrDefault(a => a.GetAttributeValue("class", "") == "posthead");

            if (postHead != null)
            {
                string id = GetIdOfPost(post);
                string postNumberAnchorID = "postcount" + id;

                var anchor = postHead.Descendants("a").FirstOrDefault(a => a.Id == postNumberAnchorID);

                if (anchor != null)
                {
                    string postNumText = anchor.GetAttributeValue("name", "");
                    int postNum = 0;
                    if (int.TryParse(postNumText, out postNum))
                        return postNum;
                }
            }

            throw new InvalidOperationException("Unable to extract a post number from the post.");
        }

        /// <summary>
        /// Gets the author of the post.
        /// </summary>
        /// <param name="post">The post to query.</param>
        /// <returns>Returns the author of the post.</returns>
        public string GetAuthorOfPost(HtmlNode post)
        {
            if (post == null)
                throw new ArgumentNullException(nameof(post));

            var postDetails = post.Elements("div").FirstOrDefault(a => a.GetAttributeValue("class", "") == "postdetails");
            var userInfo = postDetails?.Elements("div").FirstOrDefault(a => a.GetAttributeValue("class", "") == "userinfo");

            var userAnchor = userInfo?.Descendants("a").FirstOrDefault(a => a.GetAttributeValue("class", "").Contains("username"));

            return HtmlEntity.DeEntitize(userAnchor?.InnerText);
        }

        /// <summary>
        /// Get the collated text of the post, that can be used in other parts of the program.
        /// This includes BBCode formatting, where appropriate.
        /// </summary>
        /// <param name="post">The post or post content to query.</param>
        /// <returns>Returns the contents of the post as a formatted string.</returns>
        public string GetTextOfPost(HtmlNode post)
        {
            if (post == null)
                throw new ArgumentNullException(nameof(post));

            // Get the inner contents, if it wasn't passed in directly
            var postContents = GetContentsOfPost(post);

            // Start recursing at the child blockquote node.
            string postText = ExtractNodeText(postContents.Element("blockquote"));

            // Clean up the extracted text before returning.
            return CleanupPostString(postText);
        }

        #endregion

        #region Special Interface function
        public Task<int> GetStartingPostNumber(IPageProvider pageProvider, IQuest quest, CancellationToken token)
        {
            if (quest == null)
                throw new ArgumentNullException(nameof(quest));

            return Task.FromResult(quest.StartPost);
        }

        #endregion


        // Utility functions to support the above interface functions

        #region Functions dealing with pages

        /// <summary>
        /// Get the node element containing all the posts on the page.
        /// </summary>
        /// <param name="page">The page to search.</param>
        /// <returns>Returns the page element that contains the posts.</returns>
        private HtmlNode GetPostListFromPage(HtmlDocument page)
        {
            var posts = page.DocumentNode.Descendants("ol").FirstOrDefault(a => a.Id == "posts");

            return posts;
        }

        /// <summary>
        /// Given the page node containing all of the posts on the thread,
        /// get an IEnumerable list of all posts on the page.
        /// </summary>
        /// <param name="postList">Element containing posts from the page.</param>
        /// <returns>Returns a list of posts.</returns>
        private IEnumerable<HtmlNode> GetPostsFromList(HtmlNode postList)
        {
            if (postList == null)
                return new List<HtmlNode>();

            var posts = postList.Elements("li").Where<HtmlNode>(a => a.Id.StartsWith("post"));

            return posts;
        }

        /// <summary>
        /// Get a specific post from the page, when provided with the unique
        /// ID of the post.
        /// </summary>
        /// <param name="page">The page to search.</param>
        /// <param name="id">The ID of the post.</param>
        /// <returns>Returns the post.</returns>
        private HtmlNode GetPostFromPageById(HtmlDocument page, string id)
        {
            var postsList = GetPostsFromPage(page);
            string postId = "post" + id;

            return postsList.FirstOrDefault(a => a.Id == postId);
        }
        #endregion

        #region Functions dealing with posts
        /// <summary>
        /// Function to tell if the provided HTML node is a thread post.
        /// </summary>
        /// <param name="post">Proposed post.</param>
        /// <returns>Returns true if the node appears to be a legitimate post.</returns>
        private bool IsPost(HtmlNode post)
        {
            if (post == null)
                return false;

            return post.Name == "li" && post.Id.StartsWith("post");
        }

        /// <summary>
        /// Gets the inner HTML node containing the actual contents of the post.
        /// </summary>
        /// <param name="post">The post to query.</param>
        /// <returns>Returns the article node within the post.</returns>
        private HtmlNode GetContentsOfPost(HtmlNode post)
        {
            if (post == null)
                return null;

            string id = GetIdOfPost(post);

            string postMessageId = "post_message_" + id;

            var message = post.Descendants("div").FirstOrDefault(a => a.Id == postMessageId);

            return message;
        }

        #endregion

        #region Text processing functions
        /// <summary>
        /// Extract the text of the provided HTML node.  Recurses into nested
        /// divs.
        /// </summary>
        /// <param name="node">The node to pull text content from.</param>
        /// <returns>A string containing the text of the post, with formatting
        /// elements converted to BBCode tags.</returns>
        private string ExtractNodeText(HtmlNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            StringBuilder sb = new StringBuilder();

            // Search the post for valid element types, and put them all together
            // into a single string.
            foreach (var childNode in node.ChildNodes)
            {
                string nodeClass = childNode.GetAttributeValue("class", "");

                // If we encounter a quote, skip past it
                if (nodeClass.Contains("bbcode_quote"))
                    continue;

                // A <br> element adds a newline.
                // Usually redundant, but sometimes needed before we bail out on
                // nodes without any inner text (such as <br/>).
                if (childNode.Name == "br")
                {
                    sb.AppendLine("");
                    continue;
                }

                // If the node doesn't contain any text, move to the next.
                if (childNode.InnerText.Trim() == string.Empty)
                    continue;

                // Add BBCode markup in place of HTML format elements,
                // while collecting the text in the post.
                switch (childNode.Name)
                {
                    case "#text":
                        sb.Append(childNode.InnerText);
                        break;
                    case "i":
                        sb.Append("[i]");
                        sb.Append(childNode.InnerText);
                        sb.Append("[/i]");
                        break;
                    case "b":
                        sb.Append("[b]");
                        sb.Append(childNode.InnerText);
                        sb.Append("[/b]");
                        break;
                    case "u":
                        sb.Append("[u]");
                        sb.Append(childNode.InnerText);
                        sb.Append("[/u]");
                        break;
                    case "span":
                        // Keep any COLOR styles; ignore anything else, but keep the content
                        string spanStyle = childNode.GetAttributeValue("style", "");
                        Match m = spanColorRegex.Match(spanStyle);
                        if (m.Success)
                        {
                            sb.Append("[color=");
                            sb.Append(m.Groups["color"].Value);
                            sb.Append("]");
                            sb.Append(childNode.InnerText);
                            sb.Append("[/color]");
                        }
                        else
                        {
                            sb.Append(childNode.InnerText);
                        }
                        break;
                    case "a":
                        sb.Append("[url=\"");
                        sb.Append(childNode.GetAttributeValue("href", ""));
                        sb.Append("\"]");
                        sb.Append(childNode.InnerText);
                        sb.Append("[/url]");
                        break;
                    case "div":
                        // Don't Recurse into divs
                        //sb.Append(ExtractNodeText(childNode));
                        break;
                    default:
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Clean up problematic bits of text in the extracted post string.
        /// </summary>
        /// <param name="postText">The text of the post.</param>
        /// <returns>Returns a cleaned version of the post text.</returns>
        private string CleanupPostString(string postText)
        {
            if (postText == null)
                throw new ArgumentNullException(nameof(postText));

            postText = postText.TrimStart();
            postText = HtmlEntity.DeEntitize(postText);
            postText = badCharactersRegex.Replace(postText, "");
            return postText;
        }
        #endregion

    }
}

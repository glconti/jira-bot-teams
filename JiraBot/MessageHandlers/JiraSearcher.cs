#region copyright

// Copyright 2007 - 2018 Innoveo AG, Zurich/Switzerland
// All rights reserved. Use is subject to license terms.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;
using Atlassian.Jira;
using JiraBot.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace JiraBot.MessageHandlers
{
    public class JiraSearcher : IMessageHandler
    {
        private static readonly IReadOnlyList<string> InnerKeywords =
            WebConfigurationManager.AppSettings["JiraTicketKeywords"].Split(',').ToList();

        private static readonly Regex Regex = new Regex($@"(?<ticket>({string.Join("|", InnerKeywords)})-\d+)",
            RegexOptions.Compiled);

        private static string JiraAddress { get; } = WebConfigurationManager.AppSettings["JiraEndpoint"];

        private static string JiraUser { get; } = WebConfigurationManager.AppSettings["AtlassianUser"];

        private static string JiraApiToken { get; } = WebConfigurationManager.AppSettings["AtlassianAPIKey"];

        public IEnumerable<string> Keywords => InnerKeywords;

        public IEnumerable<string> Commands { get; } = Enumerable.Empty<string>();

        public async Task AnswerAsync(IDialogContext context, Activity activity, string message)
        {
            try
            {
                var matchCollection = Regex.Matches(message);
                if (matchCollection.Count == 0) return;

                var tickets = new HashSet<string>();
                foreach (Match o in matchCollection) tickets.Add(o.Groups["ticket"].Value);

                var jira = Jira.CreateRestClient(JiraAddress, JiraUser, JiraApiToken);

                var searches = tickets.Distinct().OrderBy(t => t)
                    .Select(t => new
                    {
                        Ticket = t,
                        Task = jira.Issues.GetIssueAsync(t)
                    }).ToArray();

                await Task.WhenAll(searches.Select(t => t.Task));

                var thumbnailCards = searches.Select(search =>
                {
                    var issue = search.Task.Result;

                    if (issue == null) new ThumbnailCard(subtitle: search.Ticket, text: "Not found").ToAttachment();

                    var issueNumber = issue.Key.ToString();
                    var issueUrl = issue.Jira.Url + "browse/" + issueNumber;

                    var thumbnailCard = new ThumbnailCard(
                        subtitle: issueNumber + ": " + issue.Summary,
                        text: string.Format(Resources.JiraCardTitle,
                            issue.Type.Name,
                            issue.Status.Name,
                            issue.Priority.Name,
                            issue.Assignee,
                            issueUrl));

                    return thumbnailCard.ToAttachment();
                }).ToList();

                var reply = activity.CreateReply();

                reply.AttachmentLayout = AttachmentLayoutTypes.List;
                reply.Attachments = thumbnailCards;

                await context.PostAsync(reply);
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Join(Environment.NewLine, "Error during jira search", e.Message, e.StackTrace));
            }
        }
    }
}
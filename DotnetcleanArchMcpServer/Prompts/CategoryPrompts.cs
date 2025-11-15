using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace DotnetcleanArchMcpServer.Prompts;

[McpServerPromptType]
public static class CategoryPrompts
{
    [McpServerPrompt(Name = "categories/list-plan")]
    [Description("Guides the model to gather paginated category data from the CRUD API.")]
    public static IEnumerable<ChatMessage> BuildListCategoriesPlan(
        [Description("Optional search keyword to pass along to downstream analysis.")] string? search = null,
        [Description("Optional business question to keep in mind while reviewing categories.")] string? goal = null)
    {
        var userInstructions = new StringBuilder()
            .AppendLine("Goal: inspect the existing product categories via the CRUD API.")
            .AppendLine("Use the `list_categories` tool to fetch data. Call it repeatedly if you need more than one page.")
            .AppendLine("Summarize useful insights (count, notable names, duplicates) back to the user.")
            .AppendLine("If the response is empty, clearly state that no categories were returned.");

        if (!string.IsNullOrWhiteSpace(search))
        {
            userInstructions.AppendLine()
                .AppendLine($"Focus on categories matching or related to: {search}.");
        }

        if (!string.IsNullOrWhiteSpace(goal))
        {
            userInstructions.AppendLine()
                .AppendLine($"Keep this question in mind while analyzing the data: {goal}.");
        }

        return new[]
        {
            new ChatMessage(ChatRole.System,
                "You are an internal operations analyst for the Dotnet CRUD API. " +
                "You have privileged MCP access to inspect catalog metadata."),
            new ChatMessage(ChatRole.User, userInstructions.ToString().Trim())
        };
    }

    [McpServerPrompt(Name = "categories/create-plan")]
    [Description("Helps the model craft the payload needed to create a category before calling the tool.")]
    public static IEnumerable<ChatMessage> BuildCreateCategoryPlan(
        [Description("Name of the category the user wants to add.")] string name,
        [Description("Optional description the user provided.")] string? description = null,
        [Description("Optional justification or business context.")] string? rationale = null)
    {
        var sb = new StringBuilder()
            .AppendLine("Prepare to call the `create_category` tool.")
            .AppendLine("Steps:")
            .AppendLine("1. Confirm the requested category details and ensure the name is concise.")
            .AppendLine("2. Craft the JSON arguments `{ \"name\": \"...\", \"description\": \"...\" }`.")
            .AppendLine("3. Call `create_category` with the finalized arguments.")
            .AppendLine("4. Report success or relay any errors to the user.");

        sb.AppendLine().AppendLine($"Requested name: {name}");

        if (!string.IsNullOrWhiteSpace(description))
        {
            sb.AppendLine($"Suggested description: {description}");
        }

        if (!string.IsNullOrWhiteSpace(rationale))
        {
            sb.AppendLine($"Business context: {rationale}");
        }

        return new[]
        {
            new ChatMessage(ChatRole.System,
                "You are a meticulous catalog curator. Follow the provided plan, do not skip steps."),
            new ChatMessage(ChatRole.User, sb.ToString().Trim())
        };
    }
}


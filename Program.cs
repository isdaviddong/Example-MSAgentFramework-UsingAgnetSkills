// =============================================================================
// OpenAI Agent Skills Test — Microsoft Agent Framework + OpenAI API
// Uses a hardcoded API key/model and runs the meeting-notes skill example.
//
// Usage: dotnet run
// =============================================================================

#pragma warning disable MAAI001, OPENAI001

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;

internal class Program
{
    private static async Task Main()
    {
        // Hardcoded parameters (do NOT load from config/user-secrets)
        string openAIApiKey = "REPLACE_WITH_YOUR_OPENAI_API_KEY";
        const string modelName = "gpt-4.1";

        if (openAIApiKey == "REPLACE_WITH_YOUR_OPENAI_API_KEY")
        {
            Console.Write("請輸入 OpenAI API Key：");
            var inputKey = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(inputKey))
            {
                Console.WriteLine("未提供 API Key，程式結束。");
                return;
            }

            openAIApiKey = inputKey.Trim();
        }

        var skillsDir = Path.Combine(Directory.GetCurrentDirectory(), "skills");
        if (!Directory.Exists(skillsDir))
        {
            Console.WriteLine($"找不到技能目錄：{skillsDir}");
            Console.WriteLine("請確認目前資料夾底下有 skills 目錄，或更新 skillsDir。");
            return;
        }

        var skillsProvider = new AgentSkillsProvider(skillsDir);

        AIAgent agent = new OpenAIClient(openAIApiKey)
            .GetResponsesClient()
            .AsAIAgent(new ChatClientAgentOptions
            {
                Name = "OpenAISkillsAgent",
                ChatOptions = new()
                {
                    ModelId = modelName,
                    Instructions = "你是一個樂於助人的助理，並且可以使用專門技能。",
                },
                AIContextProviders = [skillsProvider],
            }).AsBuilder()
            .Use(LogFunctionCalls)
            .Build();

        var meetingPrompt = """
            請根據以下站立會議逐字稿，整理重點與待辦事項：

            Alice：昨天我完成了登入頁面的重新設計。今天我要開始做重設密碼流程。沒有阻礙。
            Bob：我一直在除錯付款逾時問題，已經找到重試邏輯的根因。我會在中午前送出修正 PR。
            Carol：我和 Dave 一起處理搜尋 API，分頁已經完成。今天我要加上排序功能。目前卡在測試環境資料庫憑證。
            Dave：搜尋 API 的分頁我這邊也完成了。今天要寫整合測試。沒有阻礙。
            Scrum Master：Carol，我會在一小時內把測試環境憑證給你。還有其他事項嗎？好，那今天會議到這裡。
            """;

        Console.WriteLine("=== OpenAI 代理技能測試 ===\n");
        Console.WriteLine($"使用模型: {modelName}");
        Console.WriteLine($"技能目錄: {skillsDir}");
        Console.WriteLine("輸入你的問題（你可以進行一般對談，當輸入 /v 會直接貼上會議記錄逐字稿，按 Enter 離開）：");

        // 建立 session 以維持多輪對話（process 內短期記憶）
        var session = await agent.CreateSessionAsync();
        while (true)
        {
            Console.Write("你：");
            var input = Console.ReadLine();
            if (input is null)
            {
                break;
            }

            input = input.Trim();
            if (input.Length == 0 || string.Equals(input, "沒事了", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("再見！");
                break;
            }

            if (string.Equals(input, "/v", StringComparison.OrdinalIgnoreCase))
            {
                input = meetingPrompt;
                Console.WriteLine("已貼上會議記錄逐字稿。");
                Console.WriteLine(meetingPrompt);
            }

            try
            {
                AgentResponse response = await agent.RunAsync(input, session);
                Console.Write("\nAI：");
                Console.WriteLine(response.Text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n請求失敗: {ex.Message}");
            }
        }
    }

    private static async ValueTask<object?> LogFunctionCalls(
        AIAgent agent,
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
        CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n---");
        Console.WriteLine($"[Tool Call] 即將呼叫：{context.Function.Name}");
        Console.WriteLine("---");

        var result = await next(context, cancellationToken);

        Console.WriteLine("\n---");
        Console.WriteLine($"[Tool Result] {context.Function.Name} 完成");
        Console.WriteLine("---");
        // Console.WriteLine(result);
        Console.ResetColor();

        return result;
    }
}

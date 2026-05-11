# OpenAI Agent Skills Test

使用 Microsoft Agent Framework 和 OpenAI API 進行代理技能測試的專案。

## 功能概述

- **代理技能系統**：支援載入和執行自訂技能（Skills）
- **實時日誌記錄**：追蹤工具呼叫和結果
- **互動式介面**：命令列對話應用程式
- **會議記錄處理**：內建會議逐字稿解析功能

## 專案結構

```
RunningSkills/
├── main.cs              # 主程序入口
├── skills/              # 技能目錄
│   └── meeting-notes/   # 會議記錄整理技能
│       └── SKILL.md     # 技能定義檔
└── README.md           # 本檔案
```

## 技能說明

### meeting-notes
用於根據會議逐字稿整理重點、決議和待辦事項的技能。

## 使用方式

### 前置條件

- 必須安裝 .NET 10 SDK

### 執行程式

```bash
dotnet run main.cs
```

### 互動式命令

- 輸入一般問題進行對話
- 輸入 `/v` 貼上預設會議逐字稿
- 按 Enter 或輸入「沒事了」離開程式

### 範例對話

```
你：請整理會議紀錄
AI：[技能被呼叫時會顯示日誌]
```

## 技術棧

- **Runtime**：.NET 10
- **Framework**：Microsoft.Agents.AI 1.5.0
- **AI Provider**：OpenAI API
- **Language**：C# (.NET 10)
- **Key Libraries**：
  - OpenAI 2.10.0
  - Microsoft.Agents.AI.OpenAI 1.5.0
  - Microsoft.Extensions.AI

## 日誌記錄

程式會實時輸出以下信息：

- **[Tool Call]**：即將執行的工具名稱（黃色）
- **[Tool Result]**：工具執行結果
- **[Skill]**：被使用的技能名稱（如有）

## 配置

需要在 `main.cs` 中設定 OpenAI API 金鑰：

```csharp
string openAIApiKey = "your-api-key-here";
```

## 技能定義

技能透過 `skills/` 目錄下的 SKILL.md 檔案定義，包含：
- 技能名稱
- 功能說明
- 執行邏輯和指引

## 開發筆記

- 程式使用最上層陳述式（Top-level Statements）
- 技能提供者透過 `AIContextProviders` 集成
- 函數呼叫透過中介軟體進行攔截和日誌記錄

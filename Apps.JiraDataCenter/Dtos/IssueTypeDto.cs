﻿namespace Apps.Jira.Dtos;

public record IssueTypeSimpleDto(string Id, string Name);

public record IssueTypeDto(string Id, string Name, IssueTypeScopeDto? Scope);

public record IssueTypeScopeDto(string Type, ProjectScopeDto? Project);

public record ProjectScopeDto(string Id);
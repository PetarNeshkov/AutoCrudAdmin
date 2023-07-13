﻿namespace AutoCrudAdmin.Demo.Sqlite.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using AutoCrudAdmin.Controllers;
using AutoCrudAdmin.Demo.Models.Models;
using AutoCrudAdmin.Models;
using AutoCrudAdmin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NonFactors.Mvc.Grid;

public class ProjectsController
    : AutoCrudAdminController<Project>
{
    protected override IEnumerable<Func<Project, Project, AdminActionContext, ValidatorResult>> EntityValidators
        => new Func<Project, Project, AdminActionContext, ValidatorResult>[]
        {
            (_, newProject, _) => ValidateProjectNameLength(newProject),
            (_, newProject, _) => ValidateProjectNameCharacters(newProject),
        };

    protected override IEnumerable<string> ShownColumnNames
        => new[]
        {
            nameof(Project.Name),
            nameof(Project.Id),
        };

    protected override IEnumerable<GridAction> CustomActions
        => new[]
        {
            new GridAction
            {
                Action = nameof(this.This),
            },
            new GridAction
            {
                Name = nameof(this.That) + " with Id",
                Action = nameof(this.That),
            },
        };

    protected override IEnumerable<string> DateTimeFormats
        => new[]
        {
            "d/M/yyyy h:mm:ss tt",
            "d/M/yyyy H:mm:ss tt",
            "dd/MM/yyyy hh:mm:ss tt",
            "M/d/yyyy h:mm:ss tt",
            "M/d/yyyy H:mm:ss tt",
            "MM/dd/yyyy hh:mm:ss tt",
        };

    public IActionResult This()
        => this.Ok("It works!");

    public IActionResult That(string id)
        => this.Ok($"It works with Id: {id}");

    protected override IGridColumnsOf<Project> BuildGridColumns(
        IGridColumnsOf<Project> columns,
        int? stringMaxLength)
    {
        base.BuildGridColumns(columns, stringMaxLength);

        columns.Add(c => c.Tasks.Count).Titled("Tasks Count");
        columns.Add(p => string.Join(
                ", ",
                p.Tasks
                    .SelectMany(t => t.EmployeeTasks)
                    .Select(et => et.Employee)
                    .Select(e => e.Username)))
            .Titled("Project Employees");

        return columns;
    }

    protected override IQueryable<Project> ApplyIncludes(IQueryable<Project> set)
        => set.Include(x => x.Tasks)
            .ThenInclude(t => t.EmployeeTasks)
            .ThenInclude(et => et.Employee);

    private static ValidatorResult ValidateProjectNameLength(Project project)
        => project.Name.Length <= 40
            ? ValidatorResult.Success()
            : ValidatorResult.Error("Name must be at max 40 characters");

    private static ValidatorResult ValidateProjectNameCharacters(Project project)
        => project.Name.Contains('@')
            ? ValidatorResult.Error("Name cannot contain '@'")
            : ValidatorResult.Success();
}
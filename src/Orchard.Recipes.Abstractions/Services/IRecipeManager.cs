﻿using Orchard.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public interface IRecipeManager
    {
        Task ExecuteAsync(string executionId);
        Task<string> EnqueueAsync(RecipeDescriptor recipeDescriptor);
        Task<string> EnqueueAsync(string executionId, RecipeDescriptor recipeDescriptor);
    }
}
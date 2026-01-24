using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace UAlgora.Ecommerce.Tests.UI.PageObjects;

/// <summary>
/// Page object for Algora Categories management page
/// </summary>
public class AlgoraCategoriesPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public AlgoraCategoriesPage(IWebDriver driver, WebDriverWait wait)
    {
        _driver = driver;
        _wait = wait;
    }

    // Locators
    private By CategoryListContainer => By.CssSelector(".category-list, .collection-list, [class*='category']");
    private By CreateButton => By.CssSelector("button[class*='create'], .btn-create, uui-button[label='Create']");
    private By CategoryNameInput => By.CssSelector("input[name='name'], input[id*='name'], input[placeholder*='name']");
    private By CategorySlugInput => By.CssSelector("input[name='slug'], input[id*='slug'], input[placeholder*='slug']");
    private By CategoryDescriptionInput => By.CssSelector("textarea[name='description'], textarea[id*='description']");
    private By SaveButton => By.CssSelector("button[type='submit'], .btn-save, uui-button[label='Save']");
    private By DeleteButton => By.CssSelector("button[class*='delete'], .btn-delete, uui-button[label='Delete']");
    private By SearchInput => By.CssSelector("input[type='search'], input[placeholder*='Search'], .search-input");
    private By CategoryRow => By.CssSelector("tr[class*='category'], .category-item, .list-item");
    private By ConfirmDeleteButton => By.CssSelector("button[class*='confirm'], .btn-confirm, uui-button[label='Confirm']");
    private By IsVisibleToggle => By.CssSelector("input[name='isVisible'], [id*='visible'] input, .toggle-visible");

    /// <summary>
    /// Click Create Category button
    /// </summary>
    public void ClickCreateCategory()
    {
        var button = WaitForClickable(CreateButton);
        button?.Click();
        Thread.Sleep(500);
    }

    /// <summary>
    /// Fill category form
    /// </summary>
    public void FillCategoryForm(string name, string? slug = null, string? description = null, bool isVisible = true)
    {
        // Fill name
        var nameInput = WaitForElement(CategoryNameInput);
        if (nameInput != null)
        {
            nameInput.Clear();
            nameInput.SendKeys(name);
        }

        // Fill slug if provided
        if (!string.IsNullOrEmpty(slug))
        {
            var slugInput = WaitForElement(CategorySlugInput);
            if (slugInput != null)
            {
                slugInput.Clear();
                slugInput.SendKeys(slug);
            }
        }

        // Fill description if provided
        if (!string.IsNullOrEmpty(description))
        {
            var descInput = WaitForElement(CategoryDescriptionInput);
            if (descInput != null)
            {
                descInput.Clear();
                descInput.SendKeys(description);
            }
        }
    }

    /// <summary>
    /// Save category
    /// </summary>
    public void SaveCategory()
    {
        var button = WaitForClickable(SaveButton);
        button?.Click();
        Thread.Sleep(1000);
    }

    /// <summary>
    /// Search for category
    /// </summary>
    public void SearchCategory(string searchTerm)
    {
        var searchInput = WaitForElement(SearchInput);
        if (searchInput != null)
        {
            searchInput.Clear();
            searchInput.SendKeys(searchTerm);
            Thread.Sleep(500);
        }
    }

    /// <summary>
    /// Check if category exists in list
    /// </summary>
    public bool CategoryExistsInList(string categoryName)
    {
        Thread.Sleep(500);
        var categories = _driver.FindElements(CategoryRow);
        return categories.Any(c => c.Text.Contains(categoryName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Click on a category in the list
    /// </summary>
    public void ClickCategory(string categoryName)
    {
        var categories = _driver.FindElements(CategoryRow);
        var category = categories.FirstOrDefault(c => c.Text.Contains(categoryName, StringComparison.OrdinalIgnoreCase));
        category?.Click();
        Thread.Sleep(500);
    }

    /// <summary>
    /// Delete category
    /// </summary>
    public void DeleteCategory()
    {
        var deleteBtn = WaitForClickable(DeleteButton);
        deleteBtn?.Click();
        Thread.Sleep(300);

        var confirmBtn = WaitForClickable(ConfirmDeleteButton);
        confirmBtn?.Click();
        Thread.Sleep(500);
    }

    /// <summary>
    /// Get category count in list
    /// </summary>
    public int GetCategoryCount()
    {
        Thread.Sleep(500);
        return _driver.FindElements(CategoryRow).Count;
    }

    private IWebElement? WaitForElement(By locator)
    {
        try
        {
            return _wait.Until(d => d.FindElement(locator));
        }
        catch (WebDriverTimeoutException)
        {
            return null;
        }
    }

    private IWebElement? WaitForClickable(By locator)
    {
        try
        {
            return _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));
        }
        catch (WebDriverTimeoutException)
        {
            return null;
        }
    }
}

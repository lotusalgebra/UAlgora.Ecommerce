using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using UAlgora.Ecommerce.Tests.UI.Infrastructure;

namespace UAlgora.Ecommerce.Tests.UI.PageObjects;

/// <summary>
/// Page object for Algora Products management page
/// </summary>
public class AlgoraProductsPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public AlgoraProductsPage(IWebDriver driver, WebDriverWait wait)
    {
        _driver = driver;
        _wait = wait;
    }

    // Locators
    private By ProductListContainer => By.CssSelector(".product-list, .collection-list, [class*='product']");
    private By CreateButton => By.CssSelector("button[class*='create'], .btn-create, uui-button[label='Create']");
    private By ProductNameInput => By.CssSelector("input[name='name'], input[id*='name'], input[placeholder*='name']");
    private By ProductSkuInput => By.CssSelector("input[name='sku'], input[id*='sku'], input[placeholder*='SKU']");
    private By ProductPriceInput => By.CssSelector("input[name='basePrice'], input[id*='price'], input[type='number']");
    private By SaveButton => By.CssSelector("button[type='submit'], .btn-save, uui-button[label='Save']");
    private By DeleteButton => By.CssSelector("button[class*='delete'], .btn-delete, uui-button[label='Delete']");
    private By SearchInput => By.CssSelector("input[type='search'], input[placeholder*='Search'], .search-input");
    private By ProductRow => By.CssSelector("tr[class*='product'], .product-item, .list-item");
    private By ConfirmDeleteButton => By.CssSelector("button[class*='confirm'], .btn-confirm, uui-button[label='Confirm']");

    /// <summary>
    /// Click Create Product button
    /// </summary>
    public void ClickCreateProduct()
    {
        var button = WaitForClickable(CreateButton);
        button?.Click();
        Thread.Sleep(500);
    }

    /// <summary>
    /// Fill product form
    /// </summary>
    public void FillProductForm(string name, string sku, decimal price, string? description = null)
    {
        // Fill name
        var nameInput = WaitForElement(ProductNameInput);
        if (nameInput != null)
        {
            nameInput.Clear();
            nameInput.SendKeys(name);
        }

        // Fill SKU
        var skuInput = WaitForElement(ProductSkuInput);
        if (skuInput != null)
        {
            skuInput.Clear();
            skuInput.SendKeys(sku);
        }

        // Fill price
        var priceInput = WaitForElement(ProductPriceInput);
        if (priceInput != null)
        {
            priceInput.Clear();
            priceInput.SendKeys(price.ToString());
        }

        // Fill description if provided
        if (!string.IsNullOrEmpty(description))
        {
            var descInput = _driver.FindElements(By.CssSelector("textarea[name='description'], textarea[id*='description']")).FirstOrDefault();
            if (descInput != null)
            {
                descInput.Clear();
                descInput.SendKeys(description);
            }
        }
    }

    /// <summary>
    /// Save product
    /// </summary>
    public void SaveProduct()
    {
        var button = WaitForClickable(SaveButton);
        button?.Click();
        Thread.Sleep(1000);
    }

    /// <summary>
    /// Search for product
    /// </summary>
    public void SearchProduct(string searchTerm)
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
    /// Check if product exists in list
    /// </summary>
    public bool ProductExistsInList(string productName)
    {
        Thread.Sleep(500);
        var products = _driver.FindElements(ProductRow);
        return products.Any(p => p.Text.Contains(productName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Click on a product in the list
    /// </summary>
    public void ClickProduct(string productName)
    {
        var products = _driver.FindElements(ProductRow);
        var product = products.FirstOrDefault(p => p.Text.Contains(productName, StringComparison.OrdinalIgnoreCase));
        product?.Click();
        Thread.Sleep(500);
    }

    /// <summary>
    /// Delete product
    /// </summary>
    public void DeleteProduct()
    {
        var deleteBtn = WaitForClickable(DeleteButton);
        deleteBtn?.Click();
        Thread.Sleep(300);

        var confirmBtn = WaitForClickable(ConfirmDeleteButton);
        confirmBtn?.Click();
        Thread.Sleep(500);
    }

    /// <summary>
    /// Get product count in list
    /// </summary>
    public int GetProductCount()
    {
        Thread.Sleep(500);
        return _driver.FindElements(ProductRow).Count;
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

module TestHelpers

open System
open System.Threading.Tasks
open Microsoft.Playwright

/// Waits for an element to become visible
let waitForVisible (page: IPage) (selector: string) (timeoutMs: int) : Task =
    task {
        let locator = page.Locator(selector).First
        let options = LocatorWaitForOptions(State = WaitForSelectorState.Visible, Timeout = Nullable(float32 timeoutMs))
        try
            do! locator.WaitForAsync(options)
        with
        | :? PlaywrightException as ex when ex.Message.Contains("Timeout") ->
            raise (TimeoutException($"Timeout ({timeoutMs}ms) waiting for '{selector}' to become visible", ex))
    }

/// Waits for an element's text content to contain a substring
let waitForTextContains (page: IPage) (selector: string) (substring: string) (timeoutMs: int) : Task =
    task {
        // Escape single quotes for JavaScript
        let escapedSubstring = substring.Replace("'", "\\'")
        let js = $"() => document.querySelector('{selector}')?.textContent?.includes('{escapedSubstring}')"
        let options = PageWaitForFunctionOptions(Timeout = Nullable(float32 timeoutMs))
        try
            let! _ = page.WaitForFunctionAsync(js, null, options)
            ()
        with
        | :? PlaywrightException as ex when ex.Message.Contains("Timeout") ->
            raise (TimeoutException($"Timeout ({timeoutMs}ms) waiting for '{selector}' to contain '{substring}'", ex))
    }

/// Waits for an element count to match expected value
let waitForCount (page: IPage) (selector: string) (expected: int) (timeoutMs: int) : Task =
    task {
        let js = $"() => document.querySelectorAll('{selector}').length === {expected}"
        let options = PageWaitForFunctionOptions(Timeout = Nullable(float32 timeoutMs))
        try
            let! _ = page.WaitForFunctionAsync(js, null, options)
            ()
        with
        | :? PlaywrightException as ex when ex.Message.Contains("Timeout") ->
            raise (TimeoutException($"Timeout ({timeoutMs}ms) waiting for count of '{selector}' to equal {expected}", ex))
    }

/// Checks if an element is visible
let isVisible (page: IPage) (selector: string) : Task<bool> =
    task {
        let locator = page.Locator(selector)
        return! locator.IsVisibleAsync()
    }

// Original content of Controllers/AccountController.cs with line 67 updated

// ... other code ...

    public ActionResult Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Registration logic...

            // Change from Login to Index
            return RedirectToAction("Index", "Home");
        }

        // ... return some response
    }

// ... other code ...
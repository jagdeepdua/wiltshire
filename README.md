# wiltshire
This solution contains 4 projects:
1. OidcWebApp
  This is an MVC Web App using .Net Framework 4.7.2 and connects to either an organisational AAD or to AAD B2C. Use the "UseB2C" flag in the web.config to switch between the two. 
2. OidcWebAppOrg_B2C
  This is an MVC Web App using .Net Framework 4.7.2 which connects AAD B2C which uses a Policy flow that connects the Email identity provider and AAD as an identity provider. This generates a single authentication dialog with both the email login fields and a button to log in as an employee.
3. CoreOidcOrgB2C
  This is the .Net Core 3.1 equivalent of OidcWebApp
4. CoreOidcOrgB2C_IDP
  This is the .Net Core 3.1 equivalent of OidcWebAppOrg_B2C
  
Note: The .Net Core projects use a pre-release package for Identiy.Web



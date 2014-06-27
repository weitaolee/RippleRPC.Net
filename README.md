RippleRPC.Net
=============

.NET library for Ripple RPC

This is fairly rough at the moment as I figure out which of the Ripple RPC APIs are working and which are not.


There is a Tests project which shows how to use many of the commands.

For example, here's how to get information about an account:

### Account Information

            RippleClient client = new RippleClient(new Uri("http://s1.ripple.com:51234"));
            AccountInformation accountInformation = client.GetAccountInformation("rho3u4kXc5q3chQFKfn9S1ZqUCya1xT3t4);
            
            
Some of the crypto routines were adopted from the Ripple-cs project [https://github.com/bhaal275/ripple-cs]

Please post any questions or problems on the issues page.

Ripples gratefully accepted: rPGKpTsgSaQiwLpEekVj1t5sgYJiqf2HDC

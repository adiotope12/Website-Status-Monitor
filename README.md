# Website-Status-Monitor
This project is an addition to another project where I set up a website on my Raspberry Pi, which you can view here: https://www.topeadio.com/.

The purpose of this Azure Function is to constantly monitor the status of the website and notify me should the status not be available. This was inspired by my time as an automation engineering intern at Indiana Farmers Insurance. Having learned more about different types of automation, I thought it'd be a good idea to implement something similar for my site.

The function pings the site every 10 minutes and checks if a 200 status code was received. If not, then the function sends me an email. I have it set to where it can only send me one email every hour so that I don't get flooded with emails. I also used Azure Communication Services for the donotreply email and Azure Key Vaults for keeping secrets.

Thanks,
Tope

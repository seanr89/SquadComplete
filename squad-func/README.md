# TimerTrigger - C<span>#</span>

The `TimerTrigger` makes it incredibly easy to have your functions executed on a schedule. This sample demonstrates a simple use case of calling your function at 2 AM each night.

## How it works

For a `TimerTrigger` to work, you provide a schedule in the form of a [cron expression](https://en.wikipedia.org/wiki/Cron#CRON_expression)(See the link for full details). A cron expression is a string with 6 separate expressions which represent a given schedule via patterns. The pattern we use to represent 2 AM each night is `0 0 2 * * *`. This, in plain text, means: "When seconds is equal to 0, minutes is equal to 0, hour is equal to 2, for any day of the month, month, or day of the week".

## Learn more

<TODO> Documentation
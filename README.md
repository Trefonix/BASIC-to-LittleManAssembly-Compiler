# BASIC-to-LittleManAssembly-Compiler
Turns a pseudo basic into Little Man Assembly language

This is a 'compiler' which will convert a psuedo-code with inspiration from BASIC and C for syntax into LMC ASM instructions.
I wrote this program because I thought it would be pretty neat and I think it is. It was also quite fun to write

How to operate:

Allowed syntax:

-print
Usage: print a
Description: Prints the variable a to the screen

-input
Usage: input a
Description: Takes a value into the 'INBOX' and then assigns that to the variable a.

-int
Usage: int a
Description: Declares and assigns a memory location for use as a variable labelled 'a'

Selection statement:

-if statement
Usage: 	if a = b
-print b
-[optional] else
-print a
-endif

Loops:
You can have any loop as long as it's a while loop.
Usage: while a = b	
-print a
-endwhile


Allowed operators: 
'='
'>='


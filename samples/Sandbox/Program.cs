﻿using OrleanSpaces.Tuples.Typed;

//Span<char> chars = stackalloc char[48];
//bool result = new IntTuple(1, 1, 1).TryFormat(chars, out int w);

//foreach (ref readonly int item in new IntTuple(1, 1, 1))
//{
//    item = 1;
//    Console.WriteLine(item);
//}

//CharTuple chars = new CharTuple('a', 'b', 'c');
//Console.WriteLine($"{chars}");

Console.WriteLine($"{new StringTuple()}");
Console.WriteLine($"{new StringTuple("a")}");
Console.WriteLine($"{new StringTuple("ab")}");
Console.WriteLine($"{new StringTuple("a", "ab")}");
Console.WriteLine($"{new StringTuple("a", "ab", "abc")}");
Console.WriteLine($"{new StringTuple("a", "ab", "abc", "abcd")}");

Console.ReadKey();
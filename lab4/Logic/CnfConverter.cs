namespace ExpertSystems.lab4.Logic;

// Приведение формулы к КНФ (конъюнкции дизъюнктов) по алгоритму из лекции:
//   1. устранить связки импликации и эквивалентности
//   2. продвинуть отрицание до атомов (законы де Моргана)
//   3. применить закон дистрибутивности
public static class CnfConverter
{
    // приводим формулу к КНФ, печатаем шаги и возвращаем список ее дизъюнктов
    public static List<Disjunct> ToCnf(Formula formula)
    {
        var step1 = RemoveImplications(formula);    // 1. убрали -> и <->
        var step2 = PushNegation(step1, false);     // 2. отрицания стоят только перед атомами
        var step3 = Distribute(step2);              // 3. получили конъюнкцию дизъюнкций

        // печатаем только те шаги, на которых формула изменилась
        if (step1.ToString() != formula.ToString())
            Console.WriteLine($"        без импликаций:      {step1}");
        if (step2.ToString() != step1.ToString())
            Console.WriteLine($"        отрицание к атомам:  {step2}");
        if (step3.ToString() != step2.ToString())
            Console.WriteLine($"        дистрибутивность:    {step3}");

        // режем КНФ по конъюнкциям - каждая часть станет отдельным дизъюнктом
        var parts = new List<Formula>();
        Split(step3, FormulaKind.And, parts);

        var result = new List<Disjunct>();
        foreach (var part in parts)
        {
            // режем часть по дизъюнкциям - получаем атомы (со знаком)
            var literals = new List<Formula>();
            Split(part, FormulaKind.Or, literals);

            var disjunct = new Disjunct();
            foreach (var literal in literals)
            {
                // после шага 2 здесь либо атом, либо отрицание атома
                var atom = literal.Kind == FormulaKind.Not
                    ? new Atom(literal.Left!.Name, false)
                    : new Atom(literal.Name, true);

                if (!disjunct.Atoms.Any(a => a.IsSame(atom))) // повторы атома в дизъюнкте не нужны (A v A = A)
                    disjunct.Atoms.Add(atom);
            }

            if (disjunct.IsTautology()) // A v ~A всегда истинно - такой дизъюнкт ничего не дает
            {
                Console.WriteLine($"        дизъюнкт {disjunct} - тавтология, отбрасываем");
                continue;
            }

            if (!result.Any(d => d.IsSame(disjunct))) // одинаковые дизъюнкты тоже не повторяем
                result.Add(disjunct);
        }

        return result;
    }

    // шаг 1: A -> B = ~A v B;  A <-> B = (~A v B) & (~B v A)
    private static Formula RemoveImplications(Formula f)
    {
        if (f.Kind == FormulaKind.Atom)
            return f;

        var left = RemoveImplications(f.Left!);
        if (f.Kind == FormulaKind.Not)
            return new Formula(FormulaKind.Not, left);

        var right = RemoveImplications(f.Right!);
        if (f.Kind == FormulaKind.Implies)
            return new Formula(FormulaKind.Or, new Formula(FormulaKind.Not, left), right);

        if (f.Kind == FormulaKind.Equiv)
            return new Formula(FormulaKind.And,
                new Formula(FormulaKind.Or, new Formula(FormulaKind.Not, left), right),
                new Formula(FormulaKind.Or, new Formula(FormulaKind.Not, right), left));

        return new Formula(f.Kind, left, right); // & и v оставляем как есть
    }

    // шаг 2: ~(A & B) = ~A v ~B;  ~(A v B) = ~A & ~B;  ~~A = A
    // negate = true, если над текущей частью формулы висит отрицание
    private static Formula PushNegation(Formula f, bool negate)
    {
        if (f.Kind == FormulaKind.Atom)
            return negate ? new Formula(FormulaKind.Not, f) : f;

        if (f.Kind == FormulaKind.Not)
            return PushNegation(f.Left!, !negate); // отрицание уходит внутрь, двойное - сокращается

        // под отрицанием & меняется на v и наоборот
        var isAnd = (f.Kind == FormulaKind.And) != negate;
        return new Formula(isAnd ? FormulaKind.And : FormulaKind.Or,
            PushNegation(f.Left!, negate),
            PushNegation(f.Right!, negate));
    }

    // шаг 3: A v (B & C) = (A v B) & (A v C)
    private static Formula Distribute(Formula f)
    {
        if (f.Kind == FormulaKind.And)
            return new Formula(FormulaKind.And, Distribute(f.Left!), Distribute(f.Right!));

        if (f.Kind != FormulaKind.Or) // атом или отрицание атома
            return f;

        var left = Distribute(f.Left!);
        var right = Distribute(f.Right!);

        if (left.Kind == FormulaKind.And) // (B & C) v A = (B v A) & (C v A)
            return new Formula(FormulaKind.And,
                Distribute(new Formula(FormulaKind.Or, left.Left!, right)),
                Distribute(new Formula(FormulaKind.Or, left.Right!, right)));

        if (right.Kind == FormulaKind.And) // A v (B & C) = (A v B) & (A v C)
            return new Formula(FormulaKind.And,
                Distribute(new Formula(FormulaKind.Or, left, right.Left!)),
                Distribute(new Formula(FormulaKind.Or, left, right.Right!)));

        return new Formula(FormulaKind.Or, left, right);
    }

    // разрезаем формулу по связке kind: A & B & C -> [A, B, C]
    private static void Split(Formula f, FormulaKind kind, List<Formula> parts)
    {
        if (f.Kind != kind)
        {
            parts.Add(f);
            return;
        }

        Split(f.Left!, kind, parts);
        Split(f.Right!, kind, parts);
    }
}

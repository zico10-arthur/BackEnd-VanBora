# Lições Aprendidas

## 1. Jamais usar `if` / `else`

### Motivo

`if/else` aninhados reduzem legibilidade, aumentam complexidade cognitiva e dificultam manutenção. Prefira:

- **Guard clauses** (`if` com early `return`) para validações e casos de borda
- **Switch expressions / pattern matching** para decisões com múltiplos branches
- **Polimorfismo** (Strategy, State pattern) para lógica complexa

### ❌ Ruim (evitar)

```csharp
if (condicao)
{
    // ...
}
else if (outraCondicao)
{
    // ...
}
else
{
    // ...
}
```

### ✅ Bom (preferir)

#### Guard clauses

```csharp
if (erroValidacao is not null) return erroValidacao;
if (slugDuplicado) return Error.Conflict(...);
if (cpfNaoExiste) return CriarNovo();
return FazerUpgrade();
```

#### Switch expression

```csharp
return passageiro.Tipo switch
{
    TipoUsuario.Passageiro => FazerUpgrade(),
    TipoUsuario.Gerente => Error.Conflict(...),
    _ => Error.Conflict(...)
};
```

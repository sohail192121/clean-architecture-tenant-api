var repo=new TenantRepository();repo.Save("a",new Item("1","private"));repo.Save("b",new Item("1","other"));
if(repo.Get("b","1")?.Value!="other" || repo.Get("c","1") is not null)throw new Exception("tenant leak");
try{repo.Save("",new Item("1","bad"));throw new Exception("missing validation");}catch(ArgumentException){}
Console.WriteLine("PASS: identical IDs isolated across tenants and empty tenant rejected");
record Item(string Id,string Value);
interface IItems {void Save(string tenant,Item item);Item? Get(string tenant,string id);}
sealed class TenantRepository:IItems {
 readonly Dictionary<(string,string),Item> store=new();readonly object gate=new();
 static void Validate(string tenant){if(string.IsNullOrWhiteSpace(tenant))throw new ArgumentException("trusted tenant required");}
 public void Save(string tenant,Item item){Validate(tenant);if(string.IsNullOrWhiteSpace(item.Id))throw new ArgumentException("id");lock(gate)store[(tenant,item.Id)]=item;}
 public Item? Get(string tenant,string id){Validate(tenant);lock(gate)return store.GetValueOrDefault((tenant,id));}
}

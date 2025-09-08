import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { StockAggregateService } from '../../proxy/stocks/stock-aggregate.service';
import type { CreateUpdateStockAggregateDto } from '../../proxy/stocks/dtos/models';
import { ProductService } from '../../proxy/products/product.service';
import type { ProductDto, GetProductListInput, ProductVariantDto } from '../../proxy/products/dtos/models';

@Component({
  selector: 'app-stock-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './stock-form.component.html',
  styleUrls: ['./stock-form.component.css'],
})
export class StockFormComponent {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private stockService = inject(StockAggregateService);
  private productService = inject(ProductService);

  mode = signal<'create' | 'edit'>('create');
  id = signal<string | null>(null);

  vm = signal<CreateUpdateStockAggregateDto>({
    name: '',
    products: [
      { productId: '', variants: [{ productVariantId: null, quantity: 0 }] },
    ],
  });

  products = signal<ProductDto[]>([]);
  variantsPerProduct = signal<Record<number, ProductVariantDto[]>>({});
  loadingProducts = signal(false);
  loadingVariants = signal<Record<number, boolean>>({});

  title = computed(() => (this.mode() === 'create' ? 'Create Stock' : 'Edit Stock'));
  saving = signal(false);

  ngOnInit() {
    const dataMode = this.route.snapshot.data['mode'] as 'create' | 'edit' | undefined;
    if (dataMode) this.mode.set(dataMode);

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id.set(id);
      this.load(id);
    }

    this.loadProducts();
  }

  load(id: string) {
    this.stockService.get(id).subscribe(detail => {
      const vm: CreateUpdateStockAggregateDto = {
        name: detail.name,
        products: (detail.products || []).map(p => ({
          productId: p.productId,
          variants: (p.variants || []).map(v => ({ productVariantId: v.productVariantId ?? null, quantity: v.quantity })),
        })),
      };
      if (!vm.products.length) vm.products.push({ productId: '', variants: [{ productVariantId: null, quantity: 0 }] });
      this.vm.set(vm);
      vm.products.forEach((p, idx) => this.preloadVariantsFor(idx, p.productId));
    });
  }

  loadProducts() {
    this.loadingProducts.set(true);
    const input: GetProductListInput = {
      filterText: undefined,
      name: undefined,
      category: undefined,
      status: undefined,
      sorting: 'Name',
      skipCount: 0,
      maxResultCount: 1000,
    };
    this.productService.getList(input).subscribe({
      next: res => {
        this.products.set(res.items);
        this.loadingProducts.set(false);
      },
      error: _ => this.loadingProducts.set(false),
    });
  }

  onProductChange(index: number, productId: string) {
    this.vm.update(curr => {
      const clone = structuredClone(curr);
      clone.products[index].productId = productId;
      clone.products[index].variants = [{ productVariantId: null, quantity: 0 }];
      return clone;
    });
    if (!productId) {
      const map = { ...this.variantsPerProduct() };
      delete map[index];
      this.variantsPerProduct.set(map);
      return;
    }
    const loading = { ...this.loadingVariants() };
    loading[index] = true;
    this.loadingVariants.set(loading);
    this.productService.get(productId).subscribe({
      next: p => {
        const list = (p as any).variants as ProductVariantDto[] | undefined;
        const map = { ...this.variantsPerProduct() };
        map[index] = list ?? [];
        this.variantsPerProduct.set(map);
        const l = { ...this.loadingVariants() };
        l[index] = false;
        this.loadingVariants.set(l);
      },
      error: _ => {
        const l = { ...this.loadingVariants() };
        l[index] = false;
        this.loadingVariants.set(l);
      },
    });
  }

  preloadVariantsFor(index: number, productId: string) {
    if (!productId) return;
    const loading = { ...this.loadingVariants() };
    loading[index] = true;
    this.loadingVariants.set(loading);
    this.productService.get(productId).subscribe({
      next: p => {
        const list = (p as any).variants as ProductVariantDto[] | undefined;
        const map = { ...this.variantsPerProduct() };
        map[index] = list ?? [];
        this.variantsPerProduct.set(map);
        const l = { ...this.loadingVariants() };
        l[index] = false;
        this.loadingVariants.set(l);
      },
      error: _ => {
        const l = { ...this.loadingVariants() };
        l[index] = false;
        this.loadingVariants.set(l);
      },
    });
  }

  addProduct() {
    this.vm.update(curr => {
      const clone = structuredClone(curr);
      clone.products.push({ productId: '', variants: [{ productVariantId: null, quantity: 0 }] });
      return clone;
    });
  }

  removeProduct(index: number) {
    this.vm.update(curr => {
      const clone = structuredClone(curr);
      clone.products.splice(index, 1);
      if (!clone.products.length) clone.products.push({ productId: '', variants: [{ productVariantId: null, quantity: 0 }] });
      return clone;
    });
    const map = { ...this.variantsPerProduct() };
    delete map[index];
    this.variantsPerProduct.set(map);
  }

  addVariant(pIndex: number) {
    this.vm.update(curr => {
      const clone = structuredClone(curr);
      clone.products[pIndex].variants.push({ productVariantId: null, quantity: 0 });
      return clone;
    });
  }

  removeVariant(pIndex: number, vIndex: number) {
    this.vm.update(curr => {
      const clone = structuredClone(curr);
      const arr = clone.products[pIndex].variants;
      arr.splice(vIndex, 1);
      if (!arr.length) arr.push({ productVariantId: null, quantity: 0 });
      return clone;
    });
  }

  hasDuplicateVariants(pIndex: number): boolean {
    const set = new Set<string | 'null'>();
    for (const v of this.vm().products[pIndex].variants) {
      const key = (v.productVariantId ?? 'null') as any;
      if (set.has(key)) return true;
      set.add(key);
    }
    return false;
  }

  hasNegativeQuantity(pIndex: number): boolean {
    return this.vm().products[pIndex].variants.some(v => (v.quantity ?? 0) < 0);
  }

  productsInvalid(): boolean {
    const data = this.vm();
    for (let i = 0; i < data.products.length; i++) {
      if (this.hasDuplicateVariants(i) || this.hasNegativeQuantity(i) || this.hasExceedVariantStock(i)) return true;
    }
    return false;
  }

  hasExceedVariantStock(pIndex: number): boolean {
    const map = this.variantsPerProduct();
    const list = map[pIndex] || [];
    const byId = new Map<string, ProductVariantDto>();
    list.forEach(v => byId.set(v.id, v));
    for (const line of this.vm().products[pIndex].variants) {
      if (line.productVariantId) {
        const pv = byId.get(line.productVariantId);
        if (pv && typeof pv.stockQuantity === 'number' && line.quantity > pv.stockQuantity) return true;
      }
    }
    return false;
  }

  save() {
    this.saving.set(true);
    const input = this.vm();
    const invalid = this.vm().products.some((_, i) => this.hasDuplicateVariants(i) || this.hasNegativeQuantity(i) || this.hasExceedVariantStock(i));
    if (invalid) {
      this.saving.set(false);
      alert('Please fix duplicate variants, negative quantities, and quantities exceeding variant stock before saving.');
      return;
    }

    const req = this.mode() === 'create' ? this.stockService.create(input) : this.stockService.update(this.id()!, input);

    req.subscribe({
      next: _ => this.router.navigate(['/stocks']),
      error: _ => this.saving.set(false),
    });
  }

  cancel() {
    this.router.navigate(['/stocks']);
  }
}

import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, effect, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ProductService, ProductStatus } from 'src/app/proxy/products';
import { CreateUpdateProductDto, ProductDto, ProductVariantDto } from 'src/app/proxy/products/dtos';
import { ToasterService } from '@abp/ng.theme.shared';
import { LocalizationPipe, LocalizationService, ConfigStateService } from '@abp/ng.core';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, LocalizationPipe],
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.css']
})
export class ProductFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private service = inject(ProductService);
  private toaster = inject(ToasterService);
  private l = inject(LocalizationService);
  private config = inject(ConfigStateService);

  hasVariantsFeature = this.config.getFeature('MultiTenantProductManagementApp.Variants') === 'true';

  form: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    basePrice: [null],
    category: [''],
    status: [ProductStatus.Inactive],
    hasVariants: [false],
    variants: this.fb.array([] as FormGroup[]),
  });

  get variants() { return this.form.get('variants') as FormArray<FormGroup>; }

  isEdit = false;
  id: string | null = null;
  saving = false;

  ngOnInit(): void {
    this.isEdit = (this.route.snapshot.data['mode'] || '') === 'edit';
    this.id = this.route.snapshot.paramMap.get('id');

    // If variants feature is disabled, force hasVariants to false and disable the control
    if (!this.hasVariantsFeature) {
      this.form.get('hasVariants')!.setValue(false);
      this.form.get('hasVariants')!.disable();
    }

    if (this.isEdit && this.id) {
      this.load();
    }

    this.form.get('hasVariants')!.valueChanges.subscribe(v => {
      if (v) {
        this.form.get('basePrice')!.setValue(null);
      }
    });
  }

  load(){
    if (!this.id) return;
    this.service.get(this.id).subscribe({
      next: (p) => {
        this.form.patchValue({
          name: p.name,
          description: p.description,
          basePrice: p.basePrice ?? null,
          category: p.category,
          status: p.status ?? ProductStatus.Inactive,
          hasVariants: this.hasVariantsFeature ? p.hasVariants : false,
        });
        this.variants.clear();
        if (p.hasVariants && p.variants && this.hasVariantsFeature) {
          for (const v of p.variants) this.variants.push(this.createVariantGroup(v));
        }
      },
      error: err => { this.toaster.error(this.l.instant('::Product.Form.LoadFailed')); console.error(err); }
    });
  }

  createVariantGroup(v?: any){
    const group = this.fb.group({
      sku: [v?.sku || '', Validators.required],
      price: [v?.price ?? 0, [Validators.required, Validators.min(0)]],
      options: this.fb.array([] as FormGroup[])
    });
    const opts = group.get('options') as FormArray<FormGroup>;
    const vOptions = (v?.options as any[] | undefined) || [];
    if (vOptions.length){
      for (const o of vOptions){
        opts.push(this.createOptionGroup(o.name, o.value));
      }
    } else {
      opts.push(this.createOptionGroup());
    }
    this.applyOptionNameValidators(opts);
    return group;
  }

  createOptionGroup(name: string = '', value: string = ''){
    return this.fb.group({
      name: [name, [Validators.required, Validators.maxLength(64)]],
      value: [value, [Validators.required, Validators.maxLength(128)]]
    });
  }

  addVariant(){
    this.variants.push(this.createVariantGroup());
  }
  removeVariant(i: number){ this.variants.removeAt(i); }
  removeLastVariant(){
    const count = this.variants.length;
    if (count > 0) {
      this.variants.removeAt(count - 1);
    }
  }
  optionsAt(variantIndex: number){
    return this.variants.at(variantIndex).get('options') as FormArray<FormGroup>;
  }
  addOption(variantIndex: number){
    const opts = this.optionsAt(variantIndex);
    opts.push(this.createOptionGroup());
    this.applyOptionNameValidators(opts);
  }
  removeOption(variantIndex: number, optionIndex: number){
    const opts = this.optionsAt(variantIndex);
    opts.removeAt(optionIndex);
    this.applyOptionNameValidators(opts);
  }
  removeLastOption(variantIndex: number){
    const opts = this.optionsAt(variantIndex);
    if (opts.length > 0){
      opts.removeAt(opts.length - 1);
      this.applyOptionNameValidators(opts);
    }
  }

  private applyOptionNameValidators(opts: FormArray<FormGroup>){
    setTimeout(() => {
      const check = () => {
        const names = opts.controls.map(c => (c.get('name') as FormControl)?.value?.trim()?.toLowerCase() || '');
        const counts = names.reduce<Record<string, number>>((acc, n) => { if(!n) return acc; acc[n]=(acc[n]||0)+1; return acc; }, {});
        opts.controls.forEach((ctrl, i) => {
          const nameCtrl = ctrl.get('name') as FormControl;
          const hasDup = !!names[i] && counts[names[i]] > 1;
          const current = nameCtrl.errors || {};
          if (hasDup) {
            nameCtrl.setErrors({ ...current, duplicate: true });
          } else {
            if (current['duplicate']) {
              delete current['duplicate'];
              const keys = Object.keys(current);
              nameCtrl.setErrors(keys.length ? current : null);
            }
          }
        });
      };
      check();
      opts.valueChanges.subscribe(() => check());
    });
  }

  save(){
    if (this.form.invalid) return;
    this.saving = true;
    const value = this.form.value as any;
    const input: any = {
      name: value.name,
      description: value.description || undefined,
      basePrice: value.hasVariants ? null : (value.basePrice ?? null),
      category: value.category || undefined,
      status: value.status ?? ProductStatus.Inactive,
      hasVariants: this.hasVariantsFeature ? value.hasVariants : false,
      variants: (value.hasVariants && this.hasVariantsFeature) ? (value.variants as any).map((v: any) => ({
        sku: v.sku,
        price: v.price,
        options: (v.options || [])
      })) : undefined,
    };
    const obs = this.isEdit && this.id ? this.service.update(this.id, input) : this.service.create(input);
    obs.subscribe({
      next: (result) => { 
        this.toaster.success(this.isEdit ? this.l.instant('::Product.Form.ProductUpdated') : this.l.instant('::Product.Form.ProductCreated')); 
        if (!this.isEdit) {
          this.router.navigate(['/products', result.id]);
        } else {
          this.router.navigate(['/products']);
        }
      },
      error: err => { this.saving = false; this.toaster.error(this.l.instant('::Product.Form.SaveFailed')); console.error(err); }
    });
  }
}
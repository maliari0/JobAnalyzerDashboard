@Injectable({ providedIn: 'root' })
export class JobService {
  private apiUrl = '/api/job';

  constructor(private http: HttpClient) { }

  getJobs(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }
}
